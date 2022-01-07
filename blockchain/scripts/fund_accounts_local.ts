import { ethers, run, upgrades, network } from 'hardhat'
import process from 'process'

async function main() {
    if (network.config.chainId !== 31337) {
        throw Error('Ensure local chain ID is 31337. This script can only be run on the local chain. It is meant as a testing helper to ensure the infra accounts are funded for operations like contract deployment or minting.')
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
        const signers = await ethers.getSigners()
        const valued = signers[10]
        const balance = await valued.getBalance()
        for (const address of [owner, depositor, withdrawer, feesHolder]) {
            console.log(`Funding ${address}`)
            const tx = await valued.sendTransaction({
                to: address,
                value: balance.div(5)
            })
            await tx.wait()
            console.log(`Done`)
        }
    }
    catch (e) {
        console.error(e)
    }
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main().catch((error) => {
    console.error(error)
    process.exitCode = 1
})
