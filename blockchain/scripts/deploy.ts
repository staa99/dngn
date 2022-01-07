import { ethers, run, upgrades } from 'hardhat'
import process from 'process'

async function main() {
  await run('compile')

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

  console.log('Deploying')

  // We get the contract to deploy
  const implementationFactory = await ethers.getContractFactory('DigiNaira')
  const contract = await upgrades.deployProxy(
    implementationFactory,
    [withdrawer, depositor, feesHolder],
    {
      initializer: 'initialize',
    }
  )

  await contract.deployed()

  console.log('Proxy deployed to:', contract.address)
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
