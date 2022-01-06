# DNGN Keygen

A simple tool to generate the keys for the accounts managed by the DNGN infra.
A single BIP39 mnemonic is required, as well as the unique derivation path.
To increase the difficulty of deriving the keys should the mnemonic ever be breached,
a unique non-public derivation path is used.


## How to Run

Open the `keygen` directory in your terminal and run

````shell
npm install
````

Create a copy of `.env.sample` and rename it to `.env`. Set the MNEMONIC and PATH prefix accordingly.
The `DERIVATION_PATH_PREFIX` is the path excluding the account index, such that the final path for
an index `i` is formed by concatenating `DERIVATION_PATH_PREFIX` with `i`.

When the installation of the dependencies is complete and the environment is setup, simply run the following to get the keys.

````shell
npm run start
````