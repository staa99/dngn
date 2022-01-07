/* eslint-disable @typescript-eslint/no-unsafe-member-access,@typescript-eslint/no-unsafe-call,@typescript-eslint/no-explicit-any,@typescript-eslint/no-unsafe-assignment */
import { BigNumber, Contract, ethers, providers, Signer, Wallet } from 'ethers'
import { DepositMessage } from 'types'
import abi from './contract-abi.json'
import { DigiNaira } from './DigiNaira'

class Minter {
  signer: Signer
  contract: DigiNaira | undefined

  constructor() {
    const provider = new providers.JsonRpcProvider(
      process.env.BLOCKCHAIN_RPC_ENDPOINT
    )
    this.signer = new Wallet(process.env.MINTER_ACCOUNT_PRIVATE_KEY!).connect(
      provider
    )
  }

  async connect() {
    console.log('Connecting to blockchain')
    console.log('Chain ID', await this.signer.getChainId())
    console.log('Minter Balance:', await this.signer.getBalance())

    this.contract = new Contract(
      process.env.CONTRACT_ADDRESS!,
      abi,
      this.signer
    ) as DigiNaira
    console.log('Connected to blockchain')
  }

  /**
   * Mints new tokens for `tx.to`.
   * @param tx
   * @return true if the transaction completes successfully, false if a contract error occurs.
   * @throws Error when the message is invalid and shouldn't be re-queued.
   */
  async mint(tx: DepositMessage): Promise<boolean> {
    if (!this.contract) {
      throw Error('Must call Minter.initialize before minting')
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
      await this.contract.deposit(
        tx.to,
        BigNumber.from(tx.amount),
        BigNumber.from(tx.fees),
        ethers.utils.keccak256(Buffer.from(tx.offChainTransactionId))
      )
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

      if (
        errorBody?.error?.message?.indexOf('DUPLICATE_DEPOSIT_INVALID') !== -1
      ) {
        throw Error('DUPLICATE_DEPOSIT_INVALID')
      }
      return false
    }
  }
}

export default Minter
