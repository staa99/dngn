export default interface DepositMessage {
  offChainTransactionId: string
  to: string
  amount: number
  fees: number
}