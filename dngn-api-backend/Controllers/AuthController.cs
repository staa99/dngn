using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DngnApiBackend.ApiModels;
using DngnApiBackend.Exceptions;
using DngnApiBackend.Services.Auth;
using DngnApiBackend.Services.Dto;
using DngnApiBackend.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Nethereum.Signer;

namespace DngnApiBackend.Controllers
{
    [AllowAnonymous]
    [Route("auth")]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;

        public AuthController(IUserAccountRepository userAccountRepository, IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userAccountRepository = userAccountRepository;
            _configuration         = configuration;
            _logger                = logger;
        }

        [HttpGet("nonce/{address}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetNonceAsync([FromRoute] string address)
        {
            var nonce = await _userAccountRepository.GetNonceAsync(address);
            var accountType = nonce == Guid.Empty ? "NEW" : "EXISTING";

            return Ok(new
            {
                status = "success",
                type   = accountType,
                value  = nonce == Guid.Empty ? (Guid?) null : nonce
            });
        }

        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<IActionResult> ValidateNonceAsync([FromBody] LoginModel model)
        {
            AssertValidModelState();

            var account = await _userAccountRepository.GetAccountAsync(model.Address!);
            if (account == null)
            {
                _logger.LogTrace("Attempt to login without registration");
                throw new UserException("NOT_REGISTERED", "User not registered");
            }

            VerifySignatureResult(model.Address!, $"LOGIN_CODE:{account.Nonce}", model.Signature!);

            try
            {
                _logger.LogTrace("Generating new nonce for {Address} after successful validation", model.Address);
                await _userAccountRepository.GenerateNewNonceAsync(account.Id);
                _logger.LogTrace("Generated new nonce for {Address}", model.Address);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to generate new nonce for {Address}", model.Address);
                throw new ServiceException("NONCE_GENERATION_FAILED", "An error occurred while logging you in.");
            }

            var token = GenerateJwt(new JwtGenerationDto
            {
                Id      = account.Id,
                Address = account.WalletAddress
            });

            _logger.LogTrace("{Address} signed in successfully", model.Address);
            return Ok(new
            {
                status = "success",
                token,
                message = "Authenticated successfully."
            });
        }

        [HttpPost("register")]
        [Produces("application/json")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            AssertValidModelState();

            var nonce = await _userAccountRepository.GetNonceAsync(model.Address);
            if (nonce != default)
            {
                throw new UserException("ALREADY_REGISTERED", "User already registered");
            }

            VerifySignatureResult(model.Address, $"REGISTER_CODE:{model.SignedData}", model.Signature);
            ObjectId userId;

            try
            {
                var result = await DoRegisterAsync(model);
                userId = result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Address registration failed");
                throw new ServiceException("REGISTRATION_FAILED", "Failed to register your account");
            }

            var token = GenerateJwt(new JwtGenerationDto
            {
                Id      = userId,
                Address = model.Address
            });

            return Ok(new
            {
                status = "success",
                token,
                message = "Registration successful."
            });
        }

        private async Task<ObjectId> DoRegisterAsync(RegisterModel model)
        {
            _logger.LogTrace("Registering {Address}", model.Address);
            var result = await _userAccountRepository.CreateUserAccountAsync(new CreateAccountDto
            {
                WalletAddress = model.Address,
                FirstName     = model.FirstName,
                LastName      = model.LastName
            });

            if (result == default)
            {
                throw new Exception("Registration failed silently");
            }

            _logger.LogTrace("Registered {Address}", model.Address);
            return result;
        }

        private void VerifySignatureResult(string givenAddress, string signedData, string signature)
        {
            try
            {
                var messageSigner = new EthereumMessageSigner();
                var signerAddress =
                    messageSigner.EcRecover(Encoding.UTF8.GetBytes(signedData), signature);
                if (!signerAddress.Equals(givenAddress, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UserException("SIGNER_MISMATCH",
                        "The signer of the message does not match the address given");
                }
            }
            catch (BaseApplicationException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ecrecover: Signature validation failed for {Address}", givenAddress);
                throw new UserException("INVALID_SIGNATURE", "Signature is not valid");
            }
        }

        private string GenerateJwt(JwtGenerationDto dto)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, dto.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(AppJwtClaimNames.Address, dto.Address)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}