export function ensureEnvironmentVariables(): void {
  const requiredEnvKeys = [
    'AMQP_URL',
    'MINTER_ENCRYPTION_PRIVATE_KEY',
    'MINTER_ACCOUNT_PRIVATE_KEY',
    'CONTRACT_ADDRESS',
    'BLOCKCHAIN_RPC_ENDPOINT',
  ]

  const unsetKeys = requiredEnvKeys.filter(key => !process.env[key])
  if (!unsetKeys.length) {
    return
  }

  throw Error(`MINTER_LAUNCH_ERR: ${unsetKeys} not defined in environment variables`)
}