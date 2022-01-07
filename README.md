#DNGN

````text
This README will be updated with full details. A brief summary of the project is given below.
````

## Introduction

This repository contains PoC code for a Naira stablecoin infra.
The primary goal is to build a foundational open infra on which
DeFi products can be built for the Nigerian FinTech ecosystem.
It aims to solve problems associated with adoption by implementing
instant liquidity through third-party integrations for deposits
and withdrawals. It extends the EIP-20 standard to implement 
deposits and withdrawals, as well as on-chain fees. A future goal
is to also provide an on-chain solution for third-party subscriptions.
The contract uses the `Upgrades` plugin from Open Zeppelin to 
achieve 'upgradability'.


## Motivation

As a result of the multitude of middlemen in the traditional FinTech
industry, transactions go through a black box where lots of stuff happens.
Fees are driven high and transparency is basically non-existent. Lots
of bank issues end up unresolved and trust remains a problem that severely
impacts the feasibility and efficiency of online payment methods in some
environments. By switching to a decentralized and trustless system, the
benefits of transparency and trustlessness are instantly realized. Fees
can also be much lower than current systems allow. Financial privacy and
full control of access to personal financial information are other immediate
benefits. Extended benefits include the opportunity for the development of
an ecosystem around the core infrastructure that enables the services that 
users are already familiar with, except with less cost and more transparency.
It also creates the opportunity for new innovative products that can benefit
from the possibilities enabled by the infra.


## Technical Summary

The contract is based on EIP-20. Deposits and Withdrawals are enabled by an 
off-chain component that integrates with the contract. Deposits are powered
by virtual account transfers and withdrawals are triggered by transfers to
a designated wallet. The balance between total supply and the Naira reserves
is maintained by the infrastructure with the deposit and withdrawal system.