import { Contract, ethers, Wallet } from 'ethers'
import abi from './contract-abi.json'
import { DigiNaira } from './DigiNaira'

class CLIWallet {
  private signer: Wallet | undefined
  private contract: DigiNaira | undefined

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
    this.signer = this.signer?.connect(provider)
    this.contract = new Contract(contractAddress, abi, this.signer) as DigiNaira
    await this.printBalance()
  }

  async printBalance(): Promise<void> {
    this.assertContract()
    const balanceOnContract = await this.contract!.balanceOf(this.signer!.address)
    console.log('Connected to DNGN\nBalance:', balanceOnContract.toString())
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
