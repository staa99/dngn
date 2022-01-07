import { Channel, connect, Replies } from 'amqplib'
import Minter from 'blockchain'
import { rsaDecryptJson } from 'crypto-extensions'
import { DepositMessage } from 'types'
import * as process from 'process'

async function createChannel(): Promise<Channel> {
  if (!process.env.AMQP_URL) {
    throw Error('AMQP_URL must be defined in environment variables')
  }
  const connection = await connect(process.env.AMQP_URL)
  const channel = await connection.createChannel()
  await channel.prefetch(50)
  return channel
}

async function defineMinterQueue(
  channel: Channel
): Promise<Replies.AssertQueue> {
  return await channel.assertQueue('dngn.deposits.completed', {
    durable: true,
    autoDelete: false,
  })
}

async function processMessages(
  channel: Channel,
  queue: Replies.AssertQueue,
  minter: Minter
): Promise<void> {
  if (!process.env.MINTER_ENCRYPTION_PRIVATE_KEY) {
    throw Error(
      'MINTER_ENCRYPTION_PRIVATE_KEY must be defined in environment variables'
    )
  }
  const encryptionPrivateKey = process.env.MINTER_ENCRYPTION_PRIVATE_KEY

  await channel.consume(
    queue.queue,
    (msg) => {
      if (!msg) {
        return
      }

      if (!msg.content) {
        channel.nack(msg, false, false)
        return
      }

      let message: DepositMessage
      try {
        message = rsaDecryptJson<DepositMessage>(
          msg.content,
          encryptionPrivateKey
        )
      } catch (decryptionError) {
        console.error(
          'An error occurred while decrypting the message',
          decryptionError
        )
        channel.nack(msg, false, false)
        return
      }

      // eslint-disable-next-line no-void
      void (async () => {
        try {
          const result = await minter.mint(message)
          if (result) {
            channel.ack(msg, false)
          } else {
            channel.reject(msg, true)
          }
        } catch (e) {
          channel.reject(msg, false)
        }
      })()
    },
    {
      noAck: false,
      consumerTag: 'minter',
    }
  )
}

export default {
  start: async (minter: Minter): Promise<void> => {
    console.log('Connecting to messaging')
    const channel = await createChannel()
    const queue = await defineMinterQueue(channel)
    await processMessages(channel, queue, minter)
    console.log('Connected to messaging')
  },
}
