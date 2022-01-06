import * as dotenv from 'dotenv'
import {HDNode} from '@ethersproject/hdnode'
import validators from './validation'

dotenv.config()

async function main() {
  console.log('Generating Keys')
  validators.environment.checkEnvironment()

  const mnemonic = process.env.INFRA_ACCOUNTS_MNEMONIC

  let hdNode = HDNode.fromMnemonic(mnemonic!)
  let nodes = [hdNode]

  const designations = ['Owner', 'Minter', 'Withdrawer', 'Fees Holder']
  for (let i = 1; i <= 4; i++) {
    nodes[i] = nodes[0].derivePath(`${process.env.DERIVATION_PATH_PREFIX}${i}`)
    console.log(designations[i - 1])
    console.log('Private Key:', nodes[i].privateKey)
    console.log('Public Key:', nodes[i].publicKey)
    console.log('Address:', nodes[i].address)
    console.log()
  }
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})