import '@nomiclabs/hardhat-etherscan'
import '@nomiclabs/hardhat-waffle'
import '@typechain/hardhat'
import * as dotenv from 'dotenv'
import 'hardhat-gas-reporter'
import '@openzeppelin/hardhat-upgrades'

import { HardhatUserConfig, task } from 'hardhat/config'
import 'solidity-coverage'

dotenv.config()

// This is a sample Hardhat task. To learn how to create your own go to
// https://hardhat.org/guides/create-task.html
task('accounts', 'Prints the list of accounts', async (taskArgs, hre) => {
  const accounts = await hre.ethers.getSigners()

  for (const account of accounts) {
    console.log(account.address)
  }
})

if (!process.env.OWNER_PRIVATE_KEY) {
  throw Error('Owner Private Key must be set in environment variables')
}

// You need to export an object to set up your config
// Go to https://hardhat.org/config/ to learn more

const config: HardhatUserConfig = {
  solidity: '0.8.4',
  networks: {
    hardhat_local: {
      url: 'http://localhost:8545',
      accounts: [process.env.OWNER_PRIVATE_KEY],
    },
    harmony_testnet: {
      url: process.env.HARMONY_TESTNET_URL || '',
      accounts: [process.env.OWNER_PRIVATE_KEY],
    },
    harmony_mainnet: {
      url: process.env.HARMONY_MAINNET_URL || '',
      accounts: [process.env.OWNER_PRIVATE_KEY],
    },
  },
  gasReporter: {
    enabled: process.env.REPORT_GAS !== undefined,
    currency: 'USD',
  },
  etherscan: {
    apiKey: process.env.ETHERSCAN_API_KEY,
  },
}

export default config
