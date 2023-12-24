using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmallLister.Data;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers;

public class ValidateCredentialsAndCreateAccessTokenRequestHandler(
    ILogger<ValidateCredentialsAndCreateAccessTokenRequestHandler> logger,
    IApiClientRepository apiClientRepository,
    IUserAccountApiAccessRepository userAccountApiAccessRepository,
    IUserAccountTokenRepository userAccountTokenRepository,
    IConfiguration configuration)
    : IRequestHandler<ValidateCredentialsAndCreateAccessTokenRequest, GetAccessTokenResponse?>
{
    public async Task<GetAccessTokenResponse?> Handle(ValidateCredentialsAndCreateAccessTokenRequest request, CancellationToken cancellationToken)
    {
        var credentialBytes = Convert.FromBase64String(request.Credentials);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        if (credentials.Length != 2)
        {
            logger.LogInformation($"Invalid auth header [{request.Credentials}] - expected 'Basic appkey:appsecret'");
            return null;
        }

        var apiClient = await apiClientRepository.GetAsync(credentials[0]);
        if (apiClient == null)
        {
            logger.LogInformation($"Could not find api client for appkey [{credentials[0]}]");
            return null;
        }

        if (SaltHashHelper.CreateHash(credentials[1], apiClient.AppSecretSalt) != apiClient.AppSecretHash)
        {
            logger.LogInformation($"Invalid login attempt");
            return null;
        }

        var userAccountApiAccess = await userAccountApiAccessRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (userAccountApiAccess == null)
        {
            logger.LogInformation($"Unknown refresh token: {request.RefreshToken}");
            return null;
        }

        if (userAccountApiAccess.RevokedDateTime != null)
        {
            logger.LogInformation($"API Access has been revoked");
            return new GetAccessTokenResponse("user_revoked", null);
        }

        var tokenData = GuidString.NewGuidString();
        var tokenExpiry = DateTime.UtcNow.AddMinutes(30);
        await userAccountTokenRepository.CreateAsync(userAccountApiAccess, tokenData, tokenExpiry);

        var tokenOptions = configuration.GetSection("SmallListerApiJwt");
        var signingKey = tokenOptions.GetValue("SigningKey", "");
        var issuer = tokenOptions.GetValue("Issuer", "");
        var audience = tokenOptions.GetValue("Audience", "");

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, tokenData) }),
            Audience = audience,
            Issuer = issuer,
            Expires = tokenExpiry,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? "")), SecurityAlgorithms.HmacSha256Signature)
        };

        return new GetAccessTokenResponse(null, tokenHandler.WriteToken(tokenHandler.CreateToken(securityTokenDescriptor)));
    }
}