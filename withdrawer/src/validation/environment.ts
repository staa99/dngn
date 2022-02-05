export function ensureEnvironmentVariables(): void {
  const requiredEnvKeys = [
    'AMQP_URL',
    'REDIS_URL',
    'WITHDRAWER_ENCRYPTION_PRIVATE_KEY',
    'COREAPI_ENCRYPTION_PUBLIC_KEY',
    'WITHDRAWER_ACCOUNT_PRIVATE_KEY',
    'BLOCKCHAIN_RPC_ENDPOINT',
    'CONTRACT_ADDRESS',
  ]

  const unsetKeys = requiredEnvKeys.filter((key) => !process.env[key])
  if (!unsetKeys.length) {
    return
  }

  throw Error(`WITHDRAWER_LAUNCH_ERR: ${unsetKeys} not defined in environment variables`)
}
