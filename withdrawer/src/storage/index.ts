import { createClient, RedisClientType } from 'redis'

export class WithdrawerStore {
  lastBlockNumber: number
  redis: RedisClientType
  static readonly blockNumberKey = 'dev__LastBlockNumber'

  constructor() {
    this.lastBlockNumber = Number(process.env.START_BLOCK_NUMBER)
    this.redis = createClient({
      url: process.env.REDIS_URL!,
    }) as never
  }

  async connect(): Promise<void> {
    this.redis.on('error', (err) => console.log('REDIS_CLIENT_ERROR:', err))
    await this.redis.connect()
  }

  async getLastBlockNumber(): Promise<number> {
    const value = await this.redis.get(WithdrawerStore.blockNumberKey)

    if (!value) {
      await this.redis.set(WithdrawerStore.blockNumberKey, this.lastBlockNumber)
      return this.lastBlockNumber
    }

    return Number(value)
  }

  async setLastBlockNumber(blockNumber: number): Promise<void> {
    const lastBlockNumber = await this.getLastBlockNumber()
    this.lastBlockNumber = Math.max(lastBlockNumber, blockNumber)
    await this.redis.set(WithdrawerStore.blockNumberKey, this.lastBlockNumber)
  }
}
