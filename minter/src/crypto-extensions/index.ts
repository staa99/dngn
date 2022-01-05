export function rsaEncryptJson(data: Record<any, unknown>, publicKey: string): Buffer {
  const jsonString = JSON.stringify(data)
  // todo: Implement encryption here
  return Buffer.from(jsonString)
}

export function rsaDecryptJson<ResultType> (encryptedData: Buffer, privateKey: string): ResultType {
  const jsonString = Buffer.toString()
  // todo: Implement decryption here
  return JSON.parse(jsonString)
}