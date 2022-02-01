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
  
  uint256 public internalTransferFees;
  uint256 public minimumWithdrawAmount;
  uint256 public maximumWithdrawAmount;

  mapping(address => bool) public registered;
  mapping(bytes32 => bool) public withdrawals;
  mapping(bytes32 => bool) public deposits;

  struct OffChainTransaction {
    address userAddress;
    uint256 amount;
    uint256 fees;
    bytes32 offChainTransactionId;
  }

  event DepositCompleted(address indexed to, bytes32 indexed offChainTransactionId, OffChainTransaction transaction);
  event WithdrawalCompleted(address indexed from, bytes32 indexed offChainTransactionId, OffChainTransaction transaction);

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
    
    setTransferRate(500);
    setMinimumWithdrawalAmount(50000);
    setMaximumWithdrawalAmount(100000000);
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
    require(!deposits[offChainTransactionId], "DUPLICATE_DEPOSIT_INVALID");

    deposits[offChainTransactionId] = true;
    _mint(to, amount - fees);
    emit DepositCompleted(to, offChainTransactionId, OffChainTransaction(to, amount, fees, offChainTransactionId));
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
    require(!withdrawals[offChainTransactionId], "DUPLICATE_WITHDRAWAL_INVALID");

    withdrawals[offChainTransactionId] = true;
    _burn(withdrawalAddress, amount);
    emit WithdrawalCompleted(from, offChainTransactionId, OffChainTransaction(from, amount, fees, offChainTransactionId));
  }

  function register()
  public
  {
    if (registered[_msgSender()]) {
      return;
    }
    
    registered[_msgSender()] = true;
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
      require(registered[from], "UNLINKED_ACCOUNT_WITHDRAWAL");
      require(amount >= minimumWithdrawAmount, "AMOUNT_TOO_SMALL");
      require(amount >= maximumWithdrawAmount, "AMOUNT_TOO_LARGE");
    }
  }

  function calculateTransferFees(address, address to, uint256)
  public view
  returns (uint256)
  {
    require(to != withdrawalAddress, "INVALID_FEES_REQUEST");

    return internalTransferFees;
  }
  
  function setTransferRate(uint256 rate)
  public onlyRole(DEFAULT_ADMIN_ROLE)
  {
    internalTransferFees = rate;
  }
  
  function setMinimumWithdrawalAmount(uint256 amount)
  public onlyRole(DEFAULT_ADMIN_ROLE)
  {
    minimumWithdrawAmount = amount;
  }
  
  function setMaximumWithdrawalAmount(uint256 amount)
  public onlyRole(DEFAULT_ADMIN_ROLE)
  {
    maximumWithdrawAmount = amount;
  }

  function _authorizeUpgrade(address newImplementation)
  internal onlyRole(UPGRADER_ROLE) override
  {}
}