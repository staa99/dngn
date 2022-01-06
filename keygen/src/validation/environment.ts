export function ensureEnvironmentVariables(): void {
  const requiredEnvKeys = [
    'INFRA_ACCOUNTS_MNEMONIC',
    'DERIVATION_PATH_PREFIX'
  ]

  const unsetKeys = requiredEnvKeys.filter(key => !process.env[key])
  if (!unsetKeys.length) {
    return
  }

  throw Error(`KEYGEN_LAUNCH_ERR: ${unsetKeys} not defined in environment variables`)
}