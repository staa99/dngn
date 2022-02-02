import { CLIWallet } from './cli-wallet'
import { promptConfig } from './config'
import { question } from './util/cli_io'
import { Menu } from './menu'
import { ethers } from 'ethers'

async function initWallet(wallet: CLIWallet): Promise<void> {
  try {
    const importKey = await promptConfig('importKey')
    if (importKey.toLowerCase() !== 'y') {
      wallet.generateWallet()
      return
    }

    const hasPrivateKey = await promptConfig('privateKey')
    if (hasPrivateKey.toLowerCase() !== 'y') {
      const mnemonic = await question('Mnemonic: ')
      wallet.importWalletFromMnemonic(mnemonic)
      return
    }

    const privateKey = await question('Private key: ')
    wallet.importWalletFromPrivateKey(privateKey)
  } catch (e: any) {
    console.error(e)
    return await initWallet(wallet)
  }
}

async function main() {
  const wallet = new CLIWallet()
  const rpcEndpoint = await promptConfig('rpcEndpoint')
  const contractAddress = await promptConfig('contractAddress')

  await initWallet(wallet)
  await wallet.connectDNGN(rpcEndpoint, contractAddress)

  // try to login
  try {
    await wallet.login()
    console.log('Successfully logged in')
  } catch (e: any) {
    console.log('Not logged in')
  }

  const menu = new Menu({
    title: 'DNGN CLI Wallet',
    children: [
      new Menu({
        title: 'Get Balance',
        action: async () => {
          const balance = await wallet.getBalance()
          console.log('Balance:', ethers.utils.formatUnits(balance, 2))
        },
      }),
      new Menu({
        title: 'Transfer',
        action: async () => {
          const balance = await wallet.getBalance()
          console.log('Balance:', ethers.utils.formatUnits(balance, 2))
          const strAmount = await question('Enter amount: ')
          const recipient = await question('Enter recipient address: ')
          const amount = ethers.utils.parseUnits(strAmount, 2)
          await wallet.transfer(recipient, amount)
        },
      }),
      new Menu({
        title: 'Print JSON Profile',
        action: () => wallet.printProfile(),
      }),
      new Menu({
        title: 'Auth',
        children: [
          new Menu({
            title: 'Login',
            action: async () => {
              await wallet.login()
              console.log('Successfully logged in')
            },
          }),
          new Menu({
            title: 'Register',
            action: async () => {
              const firstName = await question('First name: ')
              const lastName = await question('Last name: ')

              await wallet.register(firstName, lastName)
            },
          }),
        ],
      }),
      new Menu({
        title: 'Account',
        children: [
          new Menu({
            title: 'Search Banks',
            action: async () => {
              const query = await question('Query: ')
              const banks = await wallet.listBanks(query)
              for (const bank of banks) {
                console.log(bank)
              }
            },
          }),
          new Menu({
            title: 'Set withdrawal account',
            action: async () => {
              const bankId = await question('Bank ID: ')
              const accountNumber = await question('Account Number: ')

              const accountNameLookup = await wallet.queryAccountName(bankId, accountNumber)
              const nameValid = await question(
                `Is this valid: ${accountNameLookup.data.accountName} (Y/N) [n] `
              )
              if (nameValid.toLowerCase() !== 'y') {
                throw Error('Name not valid')
              }

              await wallet.setWithdrawalAccount(bankId, accountNumber)
              console.log('Withdrawal Account Set')
            },
          }),
          new Menu({
            title: 'Generate deposit account',
            action: async () => {
              console.log(
                'Details you enter here are sent directly to the account provider. We do not store them'
              )
              const bvn = await question('BVN: ')
              const email = await question('Email Address: ')

              await wallet.generateDepositAccount(bvn, email)
            },
          }),
        ],
      }),
    ],
  })

  await menu.enter()
}

main().catch((error) => {
  console.error(error)
  process.exitCode = 1
})
