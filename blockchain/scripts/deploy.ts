import { ethers, run, upgrades } from 'hardhat'
import process from 'process'

async function main() {
  await run('compile')

  if (!process.env.MINTER_PRIVATE_KEY) {
    throw Error('Minter Private Key must be set in environment variables')
  }

  if (!process.env.WITHDRAWER_PRIVATE_KEY) {
    throw Error('Withdrawer Private Key must be set in environment variables')
  }

  if (!process.env.FEES_HOLDER_PRIVATE_KEY) {
    throw Error('Fees Holder Private Key must be set in environment variables')
  }

  const depositor = new ethers.Wallet(process.env.MINTER_PRIVATE_KEY)
  const withdrawer = new ethers.Wallet(process.env.WITHDRAWER_PRIVATE_KEY)
  const feesHolder = new ethers.Wallet(process.env.FEES_HOLDER_PRIVATE_KEY)

  console.log('Deploying')

  // We get the contract to deploy
  const implementationFactory = await ethers.getContractFactory('DigiNaira')
  const contract = await upgrades.deployProxy(
    implementationFactory,
    [withdrawer.address, depositor.address, feesHolder.address],
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
