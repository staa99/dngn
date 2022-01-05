import { ethers, run, upgrades } from 'hardhat'

async function main() {
  await run('compile')

  const [, depositor, withdrawer, feesHolder] = await ethers.getSigners()

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
