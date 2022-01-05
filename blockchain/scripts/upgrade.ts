import { ethers, run, upgrades, network } from 'hardhat'

async function main() {
  await run('compile')

  const contractAddressEnvKey = `${network.name.toUpperCase()}_CONTRACT_ADDRESS`
  const contractAddress = process.env[contractAddressEnvKey]
  if (!contractAddress) {
    throw Error('Proxy contract address must be set to upgrade implementations')
  }

  // We get the contract to deploy
  const implementationFactory = await ethers.getContractFactory('DigiNaira')
  const contract = await upgrades.upgradeProxy(
    contractAddress,
    implementationFactory
  )

  await contract.deployed()

  console.log('Proxy upgraded at:', contract.address)
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
