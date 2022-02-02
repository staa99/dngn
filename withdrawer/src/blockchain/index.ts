/* eslint-disable @typescript-eslint/no-unsafe-member-access,@typescript-eslint/no-unsafe-call,@typescript-eslint/no-explicit-any,@typescript-eslint/no-unsafe-assignment */
import EventEmitter from 'events'
import { BigNumber, Contract, ethers, providers, Signer, Wallet } from 'ethers'
import { WithdrawerStore } from 'storage'
import { OffChainTransactionMessage, WithdrawalTriggeredEventArgs } from 'types'
import abi from './contract-abi.json'
import { DigiNaira } from './DigiNaira'
import { TypedEvent } from './commons'

type TransferEvent = TypedEvent<
  [string, string, BigNumber] & { from: string; to: string; value: BigNumber }
>
class Withdrawer {
  private contract: DigiNaira | undefined
  private readonly _emitter: EventEmitter
  private readonly _signer: Signer
  private readonly _store: WithdrawerStore
  private readonly withdrawalTriggeredEvent = 'withdrawal-triggered'

  constructor(store: WithdrawerStore) {
    this._store = store
    this._emitter = new EventEmitter()
    this._signer = new Wallet(process.env.WITHDRAWER_ACCOUNT_PRIVATE_KEY!).connect(
      new providers.JsonRpcProvider(process.env.BLOCKCHAIN_RPC_ENDPOINT)
    )
  }

  async connect(): Promise<void> {
    console.log('Connecting to blockchain')
    console.log('Chain ID', await this._signer.getChainId())
    console.log('Withdrawer Balance:', await this._signer.getBalance())

    this.contract = new Contract(process.env.CONTRACT_ADDRESS!, abi, this._signer) as DigiNaira
    console.log('Connected to blockchain')
  }

  /**
   * Completes the withdrawal in tx
   * @param tx
   * @return true if the transaction completes successfully, false if a contract error occurs.
   * @throws Error when the message is invalid and shouldn't be re-queued.
   */
  async completeWithdrawal(tx: OffChainTransactionMessage): Promise<boolean> {
    if (!this.contract) {
      throw Error('Must call Withdrawer.initialize before attempting withdrawal completion')
    }

    if (tx.amount <= tx.fees) {
      throw Error('INVALID_TRANSACTION')
    }

    if (!tx.offChainTransactionId?.trim()?.length) {
      throw Error('INVALID_TRANSACTION')
    }

    if (!ethers.utils.isAddress(tx.to)) {
      throw Error('INVALID_ADDRESS')
    }

    try {
      await this.contract.withdraw(
        tx.to,
        BigNumber.from(tx.amount),
        BigNumber.from(tx.fees),
        ethers.utils.keccak256(Buffer.from(tx.offChainTransactionId))
      )
      console.log('withdrawal-successful:', tx)
      return true
    } catch (e: any) {
      console.error(e)

      // do not requeue if transaction is a duplicate
      let errorBody: any
      try {
        errorBody = JSON.parse(e.error.body)
      } catch (e) {
        return false
      }

      if (errorBody?.error?.message?.indexOf('DUPLICATE_WITHDRAWAL_INVALID') !== -1) {
        throw Error('DUPLICATE_WITHDRAWAL_INVALID')
      }
      return false
    }
  }

  async triggerEvent(transfer: TransferEvent): Promise<void> {
    const eventArgs: WithdrawalTriggeredEventArgs = {
      address: transfer.args.from,
      // the on-chain transfer limits ensure that the amount does not exceed bank transfer limits
      // which are significantly less than the maximum safe integer number
      amount: transfer.args.value.toNumber(),
      txHash: transfer.transactionHash,
    }
    console.log(
      `WithdrawalTriggered: address=${eventArgs.address}, ` +
        `amount=${eventArgs.amount.toString()}, txHash=${eventArgs.txHash}`
    )
    const tx = await transfer.getTransaction()
    await tx.wait(Number(process.env.WITHDRAWAL_CONFIRMATIONS) || 1)
    this._emitter.emit(this.withdrawalTriggeredEvent, eventArgs)
  }

  async startWithdrawalDetection(): Promise<void> {
    if (!this.contract) {
      throw Error('You must call `connect` before starting')
    }

    const startBlock = (await this._store.getLastBlockNumber()) + 1
    console.log('Pulling logs from', startBlock)
    while (true) {
      try {
        const filter = this.contract.filters.Transfer(null, await this._signer.getAddress(), null)
        const nextBlock = (await this._store.getLastBlockNumber()) + 1
        const transfers = await this.contract?.queryFilter(filter, nextBlock)

        const triggers = []
        for (const transfer of transfers) {
          if (transfer.blockNumber < nextBlock) {
            continue
          }
          triggers.push(this.triggerEvent(transfer))
        }

        if (!triggers.length) {
          await new Promise((resolve) => setTimeout((v) => resolve(v), 5000))
          continue
        }

        console.log('Processing log set')
        await Promise.all(triggers)
          .then(() => this._store.setLastBlockNumber(transfers[transfers.length - 1].blockNumber))
          .then(() => new Promise((resolve) => setTimeout((v) => resolve(v), 5000)))
          .catch((reason) => {
            // notify failure
            console.error('WITHDRAWAL_TRIGGER_FAILED', reason)
          })
      } catch (e) {
        console.error('WITHDRAWAL_POLL_ERROR', e)
      }
    }
  }

  onWithdrawalTriggered(listener: (args: WithdrawalTriggeredEventArgs) => Promise<void>): void {
    this._emitter.on(this.withdrawalTriggeredEvent, (evtArgs) =>
      setImmediate(() => {
        // eslint-disable-next-line no-void
        void listener(evtArgs)
      })
    )
  }
}

export default Withdrawer
