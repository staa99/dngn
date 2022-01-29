import { Channel, connect, Replies } from 'amqplib'
import Withdrawer from 'blockchain'
import { rsaDecryptJson, rsaEncryptObject } from 'crypto-extensions'
import { OffChainTransactionMessage, WithdrawalTriggeredEventArgs } from 'types'
import * as process from 'process'

export class Messenger {
  private readonly _withdrawer: Withdrawer
  private _channel!: Channel

  constructor(withdrawer: Withdrawer) {
    this._withdrawer = withdrawer
  }

  async connect(): Promise<void> {
    console.log('Connecting to messaging')
    this._channel = await Messenger.createChannel()
    console.log('Connected to messaging')
  }

  async startListening(): Promise<void> {
    if (!this._channel) {
      throw Error('Call `connect` before listening for messages')
    }

    const callbackQueue = await this.defineWithdrawalCallbackQueue()
    await this.processCallbackMessages(callbackQueue, this._withdrawer)
  }

  async sendTransferNotification(msg: WithdrawalTriggeredEventArgs): Promise<void> {
    if (!this._channel) {
      throw Error('Call `connect` before `sendTransferNotification`')
    }

    const triggerQueue = await this.defineWithdrawalTriggerQueue()
    this._channel.publish(
      '',
      triggerQueue.queue,
      rsaEncryptObject(msg, process.env.COREAPI_ENCRYPTION_PUBLIC_KEY!),
      {
        mandatory: true,
        persistent: true,
      }
    )
  }

  private static async createChannel(): Promise<Channel> {
    if (!process.env.AMQP_URL) {
      throw Error('AMQP_URL must be defined in environment variables')
    }
    const connection = await connect(process.env.AMQP_URL)
    const channel = await connection.createChannel()
    await channel.prefetch(50)
    return channel
  }

  private async defineWithdrawalCallbackQueue(): Promise<Replies.AssertQueue> {
    return await this._channel.assertQueue('dngn.withdrawals.completed', {
      durable: true,
      autoDelete: false,
    })
  }

  private async defineWithdrawalTriggerQueue(): Promise<Replies.AssertQueue> {
    return await this._channel.assertQueue('dngn.withdrawals.initiated', {
      durable: true,
      autoDelete: false,
    })
  }

  private async processCallbackMessages(
    queue: Replies.AssertQueue,
    withdrawer: Withdrawer
  ): Promise<void> {
    if (!process.env.WITHDRAWER_ENCRYPTION_PRIVATE_KEY) {
      throw Error('WITHDRAWER_ENCRYPTION_PRIVATE_KEY must be defined in environment variables')
    }

    await this._channel.consume(
      queue.queue,
      (msg) => {
        if (!msg) {
          return
        }

        if (!msg.content) {
          this._channel.nack(msg, false, false)
          return
        }

        let message: OffChainTransactionMessage
        try {
          message = rsaDecryptJson<OffChainTransactionMessage>(
            msg.content,
            process.env.WITHDRAWER_ENCRYPTION_PRIVATE_KEY!
          )
        } catch (decryptionError) {
          console.error('An error occurred while decrypting the message', decryptionError)
          this._channel.nack(msg, false, false)
          return
        }

        // eslint-disable-next-line no-void
        void (async () => {
          try {
            const result = await withdrawer.completeWithdrawal(message)
            if (result) {
              this._channel.ack(msg, false)
            } else {
              this._channel.reject(msg, true)
            }
          } catch (e) {
            this._channel.reject(msg, false)
          }
        })()
      },
      {
        noAck: false,
        consumerTag: 'withdrawer',
      }
    )
  }
}
