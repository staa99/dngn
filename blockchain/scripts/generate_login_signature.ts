import { ethers } from 'hardhat'
import axios from 'axios'
import * as https from 'https'

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

  let nonce: string
  try {
    const nonceResponse = await dngnApi.get(`/auth/nonce/${signer.address}`)
    nonce = nonceResponse.data.value
  } catch (e: any) {
    throw Error(
      e.response?.data?.message ?? 'An error occurred while loading the nonce'
    )
  }

  const message = `LOGIN_CODE:${nonce}`
  const signature = await signer.signMessage(message)
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
