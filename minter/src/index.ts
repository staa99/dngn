import * as dotenv from 'dotenv'
import validators from './validation'
import {default as Messaging} from './messaging'
import {default as Blockchain} from './blockchain'

dotenv.config()

async function main() {
  console.log('Starting DNGN Minter')
  validators.environment.checkEnvironment()
  await Blockchain.connect()
  await Messaging.start()
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})