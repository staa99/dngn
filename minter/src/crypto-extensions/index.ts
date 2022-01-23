import * as crypto from 'crypto'
import { Buffer } from 'buffer'

export function rsaDecryptJson<ResultType>(
  encryptedData: Buffer,
  privateKey: string
): ResultType {
  const decryptedData = crypto.privateDecrypt(
    {
      key: Buffer.from(privateKey, 'base64'),
      padding: crypto.constants.RSA_PKCS1_PADDING,
    },
    encryptedData
  )
  const jsonString = Buffer.from(decryptedData).toString()

  // eslint-disable-next-line @typescript-eslint/no-unsafe-return
  return JSON.parse(jsonString)
}
