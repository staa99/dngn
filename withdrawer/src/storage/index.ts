export class WithdrawerStore {
  lastBlockNumber: number

  constructor() {
    this.lastBlockNumber = Number(process.env.START_BLOCK_NUMBER)
  }

  async getLastBlockNumber(): Promise<number> {
    return this.lastBlockNumber
  }

  async setLastBlockNumber(blockNumber: number): Promise<void> {
    const lastBlockNumber = await this.getLastBlockNumber()
    this.lastBlockNumber = Math.max(lastBlockNumber, blockNumber)
  }
}
