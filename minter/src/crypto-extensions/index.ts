import * as crypto from 'crypto'

export function rsaDecryptJson<ResultType>(
  encryptedData: Buffer,
  privateKey: string
): ResultType {
  const decryptedData = crypto.privateDecrypt(
    {
      key: Buffer.from(privateKey),
      padding: crypto.constants.RSA_PKCS1_OAEP_PADDING,
      oaepHash: 'sha256',
    },
    encryptedData
  )
  const jsonString = Buffer.from(decryptedData).toString()

  // eslint-disable-next-line @typescript-eslint/no-unsafe-return
  return JSON.parse(jsonString)
}
