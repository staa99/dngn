import { ethers } from 'hardhat'
import { randomBytes } from 'crypto'

async function main() {
  const signers = await ethers.getSigners()
  const signer = signers[0]
  const message = `REGISTER_CODE:${randomBytes(16).toString('hex')}`
  const signature = await signer.signMessage(`REGISTER_CODE:${message}`)
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
