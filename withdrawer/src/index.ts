import * as dotenv from 'dotenv'
import Withdrawer from 'blockchain'
import validators from 'validation'
import { Messenger } from 'messaging'
import { WithdrawerStore } from 'storage'

dotenv.config()

async function main() {
  console.log('Starting DNGN Withdrawer')
  validators.environment.checkEnvironment()

  const withdrawerStore = new WithdrawerStore()
  await withdrawerStore.connect()

  const withdrawer = new Withdrawer(withdrawerStore)
  await withdrawer.connect()

  const messenger = new Messenger(withdrawer)
  await messenger.connect()
  await messenger.startListening()

  withdrawer.onWithdrawalTriggered(async (args) => {
    await messenger.sendTransferNotification(args)
  })

  await withdrawer.startWithdrawalDetection()
}

main().catch((error) => {
  console.error('Terminating From Error')
  console.error(error)
  process.exitCode = 1
})
