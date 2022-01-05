import {Channel, connect, Replies} from 'amqplib'
import {rsaDecryptJson} from '../crypto-extensions'
import {DepositMessage} from '../types'

async function createChannel(): Promise<Channel> {
  const connection = await connect(process.env.AMQP_URL!)
  const channel = await connection.createChannel()
  await channel.prefetch(50)
  return channel
}

async function defineMinterQueue(channel: Channel): Promise<Replies.AssertQueue> {
  return await channel.assertQueue('dngn.deposits.completed', {
    durable: true,
    autoDelete: false
  })
}

async function processMessages(channel: Channel, queue: Replies.AssertQueue): Promise<void> {
  await channel.consume(queue.queue, msg => {
    if (!msg) {
      return
    }

    if (!msg.content) {
      channel.nack(msg, false, false)
      return
    }

    let message: DepositMessage
    try {
      message = rsaDecryptJson<DepositMessage>(msg.content, process.env.MINTER_ENCRYPTION_PRIVATE_KEY!)
    } catch (decryptionError) {
      console.error('An error occurred while decrypting the message', decryptionError)
      channel.nack(msg, false, false)
      return
    }


  }, {
    noAck: false,
    consumerTag: 'minter',
  })
}

export default {
  start: async () => {
    console.log('Connecting to messaging')
    const channel = await createChannel()
    const queue = await defineMinterQueue(channel)
    await processMessages(channel, queue)
    console.log('Connected to messaging')
  }
}