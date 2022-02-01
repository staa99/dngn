/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import { BigNumberish, Contract, ethers, Wallet } from 'ethers'
import abi from './contract-abi.json'
import { DigiNaira } from './DigiNaira'
import https from 'https'
import axios from 'axios'
import { randomUUID } from 'crypto'

const dngnApi = axios.create({
  baseURL: 'https://localhost:5001',
  httpsAgent: new https.Agent({
    rejectUnauthorized: false,
  }),
})

export class CLIWallet {
  private signer: Wallet | undefined
  private contract: DigiNaira | undefined
  private signedIn = false

  generateWallet(): void {
    this.signer = Wallet.createRandom()
  }

  importWalletFromMnemonic(mnemonic: string): void {
    if (ethers.utils.isValidMnemonic(mnemonic)) {
      throw Error('Invalid Mnemonic')
    }

    this.signer = Wallet.fromMnemonic(mnemonic)
  }

  importWalletFromPrivateKey(key: string): void {
    this.signer = new Wallet(key)
  }

  exportPrivateKey(): string {
    this.assertSigner()
    return this.signer!.privateKey
  }

  async connectDNGN(rpcEndpoint: string, contractAddress: string): Promise<void> {
    this.assertSigner()
    const provider = new ethers.providers.JsonRpcProvider(rpcEndpoint)
    this.signer = this.signer!.connect(provider)
    this.contract = new Contract(contractAddress, abi, this.signer) as DigiNaira
    const balance = await this.getBalance()
    console.log('Connected to DNGN\nBalance:', ethers.utils.formatUnits(balance, 2))
  }

  async getBalance(): Promise<BigNumberish> {
    this.assertContract()
    return await this.contract!.balanceOf(this.signer!.address)
  }

  async transfer(recipientAddress: string, amount: BigNumberish): Promise<void> {
    this.assertContract()
    try {
      const tx = await this.contract!.transfer(recipientAddress, amount)
      await tx.wait()
    } catch (e: any) {
      console.error(e)
      throw Error('Transfer failed')
    }
  }

  async login(): Promise<void> {
    this.assertContract()
    try {
      const nonceResponse = await dngnApi.get(`/auth/nonce/${this.signer!.address}`)
      const message = `LOGIN_CODE:${nonceResponse.data.nonce}`
      const signature = await this.signer!.signMessage(message)

      const loginResponse = await dngnApi.post('/auth/login', {
        address: this.signer!.address,
        signature: signature,
      })

      if (loginResponse.data.token) {
        dngnApi.defaults.headers.common.Authorization = `Bearer ${loginResponse.data.token}`
        this.signedIn = true
      }
    } catch (e: any) {
      throw Error(e.response?.data?.error?.message ?? 'Invalid credentials')
    }
  }

  async register(firstName: string, lastName: string): Promise<void> {
    this.assertContract()
    const message = randomUUID()
    const signature = await this.signer!.signMessage(`REGISTER_CODE:${message}`)
    try {
      await dngnApi.post(`/auth/register`, {
        address: this.signer!.address,
        firstName: firstName,
        lastName: lastName,
        signature: signature,
        signedData: message,
      })
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Web registration failed')
    }

    try {
      await this.login()
      console.log('Web registration done')
      const registered = await this.contract!.registered(this.signer!.address)
      if (registered) {
        console.log('Blockchain registration done')
        return
      }

      const tx = await this.contract!.register()
      await tx.wait()
      console.log('Blockchain registration done')
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Blockchain Registration Failed')
    }
  }

  async generateDepositAccount(bvn: string, email: string): Promise<void> {
    if (!this.signedIn) {
      console.error('Only authenticated users can generate deposit accounts')
    }

    try {
      const result = await dngnApi.post('/users/deposit-account', {
        emailAddress: email,
        bvn,
      })
      console.log(result.data.message)
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Deposit Account Generation Failed')
    }
  }

  async setWithdrawalAccount(bankId: string, accountNumber: string): Promise<void> {
    try {
      await dngnApi.post('/users/withdrawal-account', {
        bankId,
        accountNumber,
      })
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Failed to set withdrawal account')
    }
  }

  async queryAccountName(bankId: string, accountNumber: string): Promise<any> {
    try {
      const response = await dngnApi.get(
        `/banks/account-name?bankId=${bankId}&accountNumber=${accountNumber}`
      )
      return response.data
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Deposit Account Generation Failed')
    }
  }

  async listBanks(query: string): Promise<string[]> {
    try {
      const bankResponse = await dngnApi.get(`/banks?query=${query}`)
      return bankResponse.data.data?.map((d: any) => `${d.id}: ${d.name}`)
    } catch (e: any) {
      console.error(e.response?.data?.error?.message ?? 'Failed to load banks')
      throw Error('Failed to load banks')
    }
  }

  private assertSigner() {
    if (!this.signer) {
      throw new Error('NO_ACTIVE_WALLET')
    }
  }

  private assertContract() {
    if (!this.contract) {
      throw new Error('CONTRACT_NOT_CONNECTED')
    }
  }
}
