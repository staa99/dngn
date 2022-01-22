import { ethers } from 'hardhat'
import { randomUUID } from 'crypto'

async function main() {
  const signers = await ethers.getSigners()
  const signer = signers[0]
  const message = `REGISTER_CODE:${randomUUID()}`
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
