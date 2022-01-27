import { SignerWithAddress } from '@nomiclabs/hardhat-ethers/dist/src/signer-with-address'
import { expect } from 'chai'
import { BigNumber, BigNumberish } from 'ethers'
import { keccak256 } from 'ethers/lib/utils'
import { ethers } from 'hardhat'
import { DigiNaira } from '../typechain'

const zeroAddress = '0x0000000000000000000000000000000000000000'

const initContract = async (): Promise<DigiNaira> => {
  const [, depositor, withdrawer, feesHolder] = await ethers.getSigners()
  const contractFactory = await ethers.getContractFactory('DigiNaira')
  const contract = (await contractFactory.deploy()) as unknown as DigiNaira
  await contract.deployed()

  const tx = await contract.initialize(
    withdrawer.address,
    depositor.address,
    feesHolder.address
  )
  await tx.wait()
  return contract
}

describe('DigiNaira', () => {
  let admin: SignerWithAddress,
    depositor: SignerWithAddress,
    withdrawer: SignerWithAddress,
    feesHolder: SignerWithAddress,
    signer1: SignerWithAddress,
    signer2: SignerWithAddress,
    contract: DigiNaira

  before(async () => {
    [admin, depositor, withdrawer, feesHolder, signer1, signer2] =
      await ethers.getSigners()
  })

  beforeEach(async () => {
    contract = (await initContract()).connect(signer1)
  })

  describe('deposit', () => {
    const amount = 20000
    const fees = 600
    const txId = keccak256(Buffer.from('transaction1'))

    beforeEach(() => {
      contract = contract.connect(depositor)
    })

    it('should mint new tokens for address and update deposits mapping', async () => {
      const tx = await contract.deposit(signer1.address, amount, fees, txId)
      await tx.wait()

      // eslint-disable-next-line no-unused-expressions
      expect(await contract.deposits(txId)).to.be.true
      expect(await contract.totalSupply()).to.equal(
        BigNumber.from(amount - fees)
      )
      expect(await contract.balanceOf(signer1.address)).to.equal(
        BigNumber.from(amount - fees)
      )
    })

    it('should emit Transfer event from zero address to user address', async () => {
      expect(contract.deposit(signer1.address, amount, fees, txId))
        .to.emit(contract, 'Transfer')
        .withArgs(zeroAddress, signer1.address, BigNumber.from(amount - fees))
    })

    it('should emit DepositCompleted event with transaction data', async () => {
      expect(contract.deposit(signer1.address, amount, fees, txId))
        .to.emit(contract, 'DepositCompleted')
        .withArgs(signer1.address, txId, [signer1.address, amount, fees, txId])
    })

    it('should revert when permission not granted', async () => {
      // eslint-disable-next-line no-unused-expressions
      expect(
        contract.connect(signer2).deposit(signer1.address, amount, fees, txId)
      ).to.be.reverted
    })
  })

  describe('withdraw', () => {
    const depositAmount = 20000
    const depositFees = 600
    const withdrawalAmount = depositAmount - depositFees
    const withdrawalFees = 200
    const depositTxId = keccak256(Buffer.from('transaction1'))
    const withdrawalTxId = keccak256(Buffer.from('transaction2'))

    beforeEach(async () => {
      // deposit some money
      contract = contract.connect(depositor)
      let tx = await contract.deposit(
        signer1.address,
        depositAmount,
        depositFees,
        depositTxId
      )
      await tx.wait()

      // transfer to withdrawer to initiate withdrawal
      contract = contract.connect(signer1)
      tx = await contract.register()
      await tx.wait()

      tx = await contract.transfer(withdrawer.address, withdrawalAmount)
      await tx.wait()

      // switch to withdrawer
      contract = contract.connect(withdrawer)
    })

    it('should burn withdrawn tokens and update withdrawals mapping', async () => {
      const tx = await contract.withdraw(
        signer1.address,
        withdrawalAmount,
        withdrawalFees,
        withdrawalTxId
      )
      await tx.wait()
      // eslint-disable-next-line no-unused-expressions
      expect(await contract.withdrawals(withdrawalTxId)).to.be.true
      expect(await contract.totalSupply()).to.equal(BigNumber.from(0))
      expect(await contract.balanceOf(signer1.address)).to.equal(
        BigNumber.from(0)
      )
    })

    it('should emit Transfer event from withdrawal address to zero address', async () => {
      expect(
        contract.withdraw(
          signer1.address,
          withdrawalAmount,
          withdrawalFees,
          withdrawalTxId
        )
      )
        .to.emit(contract, 'Transfer')
        .withArgs(
          withdrawer.address,
          zeroAddress,
          BigNumber.from(withdrawalAmount)
        )
    })

    it('should emit WithdrawalCompleted event with transaction data', async () => {
      expect(
        contract.withdraw(
          signer1.address,
          withdrawalAmount,
          withdrawalFees,
          withdrawalTxId
        )
      )
        .to.emit(contract, 'WithdrawalCompleted')
        .withArgs(signer1.address, withdrawalTxId, [
          signer1.address,
          withdrawalAmount,
          withdrawalFees,
          withdrawalTxId,
        ])
    })

    it('should revert when permission not granted', async () => {
      // eslint-disable-next-line no-unused-expressions
      expect(
        contract
          .connect(signer2)
          .withdraw(
            signer1.address,
            withdrawalAmount,
            withdrawalFees,
            withdrawalTxId
          )
      ).to.be.reverted
    })

    it('should revert when amount <= fees', async () => {
      // eslint-disable-next-line no-unused-expressions
      expect(
        contract.withdraw(signer1.address, 100, 100, withdrawalTxId)
      ).to.be.revertedWith('INVALID_AMOUNT')
    })

    it('should revert on duplicate transactions', async () => {
      contract.withdraw(
        signer1.address,
        withdrawalAmount,
        withdrawalFees,
        withdrawalTxId
      )
      expect(
        contract.withdraw(
          signer1.address,
          withdrawalAmount,
          withdrawalFees,
          withdrawalTxId
        )
      ).to.be.revertedWith('DUPLICATE_WITHDRAWAL_INVALID')
    })
  })

  describe('transfer', () => {
    const depositAmount = 20000
    const depositFees = 600
    const transferAmount = 10000
    const depositTxId = keccak256(Buffer.from('transaction1'))
    let transferFees: BigNumberish

    beforeEach(async () => {
      // deposit some money
      contract = contract.connect(depositor)
      const tx = await contract.deposit(
        signer1.address,
        depositAmount,
        depositFees,
        depositTxId
      )
      await tx.wait()

      contract = contract.connect(signer1)
      transferFees = await contract.calculateTransferFees(
        signer1.address,
        signer2.address,
        transferAmount
      )
    })

    it('should update balances and total supply', async () => {
      const tx = await contract.transfer(signer2.address, transferAmount)
      await tx.wait()

      expect(await contract.totalSupply()).to.equal(
        BigNumber.from(depositAmount - depositFees)
      )
      expect(await contract.balanceOf(signer1.address)).to.equal(
        BigNumber.from(depositAmount - depositFees)
          .sub(transferAmount)
          .sub(transferFees)
      )
      expect(await contract.balanceOf(signer2.address)).to.equal(
        BigNumber.from(transferAmount)
      )
    })

    it('should emit Transfer event from sender address to receiver address', async () => {
      expect(contract.transfer(signer2.address, transferAmount))
        .to.emit(contract, 'Transfer')
        .withArgs(
          signer1.address,
          signer2.address,
          BigNumber.from(transferAmount)
        )
    })

    it('should emit Transfer event from sender address to fees holder address', async () => {
      expect(contract.transfer(signer2.address, transferAmount))
        .to.emit(contract, 'Transfer')
        .withArgs(signer1.address, feesHolder.address, transferFees)
    })
  })
})
