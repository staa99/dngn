# DNGN Withdrawer Microservice

This microservice detects withdrawal events on the blockchain and
notifies the rest of the system to process the withdrawals. It also
listens for updates to finalize withdrawals on the chain.

Messages are encrypted with RSA.
The callback private key and the notification public key are stored 
in an environment variable on the withdrawer environment.

Beofre encryption, notification messages are of the form:

```json
{
  "txId": "transaction id on blockchain",
  "blockNumber": "The block number",
  "amount": "total amount withdrawn",
  "address": "withdrawer address"
}
```

On decryption, messages are UTF-8 encoded JSON strings with the following format:

```json
{
  "offChainTransactionId": "transaction id from deposit provider",
  "to": "depositor's address",
  "amount": "total amount from depositor (integer kobo)",
  "fees": "computed platform fees (integer kobo)"
}
```

The private key for the withdrawer and the contract address of the DNGN
contract are stored as environment variables on the minter environment.

The withdrawer notifies the core API of any withdrawal transfers and then burns 
the tokens as soon as the transfer is completed successfully.

If the burn fails because the withdrawer has insufficient balance for gas, the status is logged 
and the burner delays until the balance is restored. A notification is sent (just stubbed in the PoC)
via email to alert the provider of the status.

If it fails because of an invalid message, it is rejected and dropped.

If it fails because of a different issue, the error is logged and a notification is sent.
Then the withdrawer rejects the message and re-queues it.
