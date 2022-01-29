import { ethers } from 'hardhat'
import { randomUUID } from 'crypto'
import axios from 'axios'
import * as https from 'https'
import { Contract } from 'ethers'
import abi from './contract-abi.json'

const dngnApi = axios.create({
  baseURL: process.env.DNGN_API_URL,
  httpsAgent: new https.Agent({
    rejectUnauthorized:
      !process.env.DNGN_API_URL?.startsWith('https://localhost'),
  }),
})

async function main() {
  if (!process.env.DNGN_API_URL) {
    throw Error('DNGN_API_URL must be set in config')
  }

  const signers = await ethers.getSigners()
  const signer = signers[0]
  const message = `REGISTER_CODE:${randomUUID()}`
  const signature = await signer.signMessage(message)

  const contract = new Contract(process.env.CONTRACT_ADDRESS!, abi, signer)

  try {
    await dngnApi.post(`/auth/register`, {
      address: signer.address,
      firstName: 'Cyber1',
      lastName: 'Tester',
      signature: signature,
      signedData: message.substring(14),
    })
  } catch (e) {
    console.log('Web registration failed')
  }

  console.log('Web registration Complete')
  console.log('Balance:', await contract.balanceOf(signer.address))
  const tx = await contract.register()
  const txReceipt = await tx.wait()
  console.log('Blockchain registration Complete', txReceipt)

  console.log(
    `Address: ${signer.address}\nMessage: ${message}\nSignature: ${signature}`
  )
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
