import * as crypto from 'crypto'
import { Buffer } from 'buffer'

export function rsaDecryptJson<ResultType>(encryptedData: Buffer, privateKey: string): ResultType {
  const jsonString = crypto
    .privateDecrypt(
      {
        key: Buffer.from(privateKey, 'base64'),
        padding: crypto.constants.RSA_PKCS1_PADDING,
      },
      encryptedData
    )
    .toString()

  // eslint-disable-next-line @typescript-eslint/no-unsafe-return
  return JSON.parse(jsonString)
}

export function rsaEncryptObject<ParameterType>(obj: ParameterType, publicKey: string): Buffer {
  const jsonString = JSON.stringify(obj)

  return crypto.publicEncrypt(
    {
      key: Buffer.from(publicKey, 'base64'),
      padding: crypto.constants.RSA_PKCS1_PADDING,
    },
    Buffer.from(jsonString)
  )
}
