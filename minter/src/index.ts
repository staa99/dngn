import * as dotenv from 'dotenv'
import Minter from 'blockchain'
import validators from 'validation'
import Messaging from 'messaging'

dotenv.config()

async function main() {
  console.log('Starting DNGN Minter')
  validators.environment.checkEnvironment()
  const minter = new Minter()
  await minter.connect()
  await Messaging.start(minter)
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
