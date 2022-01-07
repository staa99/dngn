import { ethers, network } from 'hardhat'
import process from 'process'

async function getValuedAccount() {
  const signers = await ethers.getSigners()
  for (let i = 1; i < signers.length; i++) {
    const valued = signers[i]
    const balance = await valued.getBalance()
    if (balance.gte(ethers.utils.parseEther('0.1'))) {
      return valued
    }
  }

  throw Error(
    `No valued account found from the default signers list of ${signers.length}`
  )
}

async function main() {
  if (network.name !== 'localhost') {
    throw Error(
      `Network must be localhost.\nThis script can only be run on the local chain. It is meant as a testing helper to ensure the infra accounts are funded for operations like contract deployment or minting.`
    )
  }

  if (!process.env.OWNER_PRIVATE_KEY) {
    throw Error('Owner Private Key must be set in environment variables')
  }

  if (!process.env.MINTER_ADDRESS) {
    throw Error('Minter Address must be set in environment variables')
  }

  if (!process.env.WITHDRAWER_ADDRESS) {
    throw Error('Withdrawer Address must be set in environment variables')
  }

  if (!process.env.FEES_HOLDER_ADDRESS) {
    throw Error('Fees Holder Address must be set in environment variables')
  }

  const depositor = process.env.MINTER_ADDRESS
  const withdrawer = process.env.WITHDRAWER_ADDRESS
  const feesHolder = process.env.FEES_HOLDER_ADDRESS
  const owner = ethers.utils.computeAddress(process.env.OWNER_PRIVATE_KEY)

  try {
    const valued = await getValuedAccount()
    const balance = await valued.getBalance()

    for (const address of [owner, depositor, withdrawer, feesHolder]) {
      console.log(`Funding ${address}`)
      const tx = await valued.sendTransaction({
        to: address,
        value: balance.div(5),
      })
      await tx.wait()
      console.log(`Done`)
    }
  } catch (e) {
    console.error(e)
  }
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
