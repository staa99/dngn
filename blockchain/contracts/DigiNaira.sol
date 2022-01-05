// SPDX-License-Identifier: GPLv3
pragma solidity ^0.8.4;

import "@openzeppelin/contracts-upgradeable/token/ERC20/ERC20Upgradeable.sol";
import "@openzeppelin/contracts-upgradeable/security/PausableUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/access/AccessControlUpgradeable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/Initializable.sol";
import "@openzeppelin/contracts-upgradeable/proxy/utils/UUPSUpgradeable.sol";

/// @custom:security-contact ahmadulfawwaz@gmail.com
contract DigiNaira is Initializable, ERC20Upgradeable, PausableUpgradeable, AccessControlUpgradeable, UUPSUpgradeable {
  bytes32 public constant PAUSER_ROLE = keccak256("PAUSER_ROLE");
  bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");
  bytes32 public constant UPGRADER_ROLE = keccak256("UPGRADER_ROLE");
  bytes32 public constant WITHDRAWER_ROLE = keccak256("WITHDRAWER_ROLE");

  address public withdrawalAddress;
  address public feesAddress;

  mapping(bytes32 => OffChainTransaction) public deposits;
  mapping(bytes32 => OffChainTransaction) public withdrawals;

  /**
  * A mapping of addresses to the keccak256 hash of the account IDs on the off-chain account storage
  */
  mapping(address => bytes32) public withdrawalAccounts;

  struct OffChainTransaction {
    address userAddress;
    uint256 amount;
    uint256 fees;
    bytes32 offChainTransactionId;
  }

  event WithdrawalCompleted(address indexed from, OffChainTransaction transaction);

  function initialize(address _withdrawalAddress, address _depositorAddress, address _feesAddress)
  public initializer
  {
    __ERC20_init("DigiNaira", "DNGN");
    __Pausable_init();
    __AccessControl_init();
    __UUPSUpgradeable_init();

    _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    _grantRole(PAUSER_ROLE, msg.sender);
    _grantRole(UPGRADER_ROLE, msg.sender);

    _grantRole(MINTER_ROLE, _depositorAddress);
    _grantRole(WITHDRAWER_ROLE, _withdrawalAddress);
    withdrawalAddress = _withdrawalAddress;
    feesAddress = _feesAddress;
  }

  function decimals()
  public view virtual override
  returns (uint8)
  {
    return 2;
  }

  function deposit(address to, uint256 amount, uint256 fees, bytes32 offChainTransactionId)
  public onlyRole(MINTER_ROLE)
  {
    require(amount > fees, "INVALID_AMOUNT");
    deposits[offChainTransactionId] = OffChainTransaction(to, amount, fees, offChainTransactionId);
    _mint(to, amount - fees);
  }

  /**
  * Logs a valid withdrawal record.
  *
  * Requirements:
  * 1. Only callable by the with
  * 1. Amount must be inclusive of fees and cannot be zero (amount must be greater than fees).
  * 2. Withdrawal must not have been recorded before.
  */
  function withdraw(address from, uint256 amount, uint256 fees, bytes32 offChainTransactionId)
  public onlyRole(WITHDRAWER_ROLE)
  {
    require(amount > fees, "INVALID_AMOUNT");
    require(withdrawals[offChainTransactionId].userAddress == address(0), "DUPLICATE_WITHDRAWAL_INVALID");

    withdrawals[offChainTransactionId] = OffChainTransaction(from, amount, fees, offChainTransactionId);
    _burn(withdrawalAddress, amount);
    emit WithdrawalCompleted(from, withdrawals[offChainTransactionId]);
  }

  function register(bytes32 withdrawalAccountId)
  public
  {
    require(withdrawalAccountId != 0, "INVALID_ACCOUNT");
    withdrawalAccounts[_msgSender()] = withdrawalAccountId;
  }

  function pause()
  public onlyRole(PAUSER_ROLE)
  {
    _pause();
  }

  function unpause()
  public onlyRole(PAUSER_ROLE)
  {
    _unpause();
  }

  /**
     * @dev See {IERC20-transfer}.
     *
     * Requirements:
     *
     * - `recipient` cannot be the zero address.
     * - the caller must have a balance of at least `amount`.
     */
  function transfer(address recipient, uint256 amount)
  public virtual override
  returns (bool)
  {
    address from = _msgSender();
    super.transfer(recipient, amount);

    // withdrawal fees are charged off-chain
    if (recipient != withdrawalAddress)
    {
      super.transfer(feesAddress, calculateTransferFees(from, recipient, amount));
    }
    return true;
  }

  function _beforeTokenTransfer(address from, address to, uint256 amount)
  internal whenNotPaused override
  {
    super._beforeTokenTransfer(from, to, amount);
    if (to == withdrawalAddress) {
      require(withdrawalAccounts[from] != 0, "UNLINKED_ACCOUNT_WITHDRAWAL");
    }
  }

  function calculateTransferFees(address, address to, uint256)
  public view
  returns (uint256)
  {
    require(to != withdrawalAddress, "INVALID_FEES_REQUEST");

    // change here to charge on-chain fees
    return 0;
  }

  function _authorizeUpgrade(address newImplementation)
  internal onlyRole(UPGRADER_ROLE) override
  {}
}