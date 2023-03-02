using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Utils.Auth.JwtHelper
{
    public sealed class JwtTokenBuilder
    {
        private readonly Dictionary<string, string> _claims = new Dictionary<string, string>();
        private string _audience;
        private int _expiryInMins = 30;
        private string _issuer;
        private string _refreshToken;
        private SecurityKey _securityKey;
        private string _systemIdentity;
        private int _tag = -1;
        private long _creds = -1;

        public JwtTokenBuilder(IOptions<JWTOptions> configuration)
        {
            _issuer = configuration.Value.Issuer;
            _audience = configuration.Value.Audience;
            _securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.Value.SecretKey));
            _expiryInMins = configuration.Value.TokenTimeout;
        }

        public JwtTokenBuilder AddSecurityKey(SecurityKey securityKey)
        {
            _securityKey = securityKey;
            return this;
        }

        public JwtTokenBuilder AddSubject(string subject)
        {
            _claims.Add(ClaimTypes.NameIdentifier, subject);
            return this;
        }

        public JwtTokenBuilder AddIssuer(string issuer)
        {
            _issuer = issuer;
            return this;
        }

        public JwtTokenBuilder AddAudience(string audience)
        {
            _audience = audience;
            return this;
        }

        public JwtTokenBuilder AddClaim(string type, string value)
        {
            if (!string.IsNullOrEmpty(value)) _claims.Add(type, value);
            return this;
        }

        public JwtTokenBuilder AddId(string value)
        {
            _claims.Add("ID", value);
            return this;
        }

        public JwtTokenBuilder AddCardNumber(string value)
        {
            _claims.Add("CardNumber", value);
            return this;
        }

        public JwtTokenBuilder AddTag(int value)
        {
            _tag = value;
            return this;
        }

        public JwtTokenBuilder AddCreds(long value)
        {
            _creds = value;
            return this;
        }

        public JwtTokenBuilder RemoveTag(string type)
        {
            _claims.Remove(type);
            return this;
        }

        public JwtTokenBuilder AddRole(Type type, string value)
        {
            _claims.Add(type.Name, value);
            return this;
        }

        public JwtTokenBuilder AddProductId(string productId)
        {
            _claims.Add("PID", productId);
            return this;
        }

        public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
        {
            _claims.Union(claims);
            return this;
        }

        public void AddMoneyTransAuthRole(Type type, string value)
        {
            var state = Enum.Parse(type, value);
            _claims.Add("MoneyTransferState", ((int)state).ToString());
        }

        public JwtTokenBuilder AddExpiryInMins(int expireyInMin)
        {
            _expiryInMins = expireyInMin;
            return this;
        }

        public JwtTokenBuilder AddRefreshToken(string refreshToken)
        {
            _refreshToken = refreshToken;
            return this;
        }

        public JwtTokenBuilder AddIdentity(string identity)
        {
            _systemIdentity = identity;
            return this;
        }

        public JwtToken Build()
        {
            EnsureArguments();

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
                .Union(_claims.Select(item => new Claim(item.Key, item.Value)));

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryInMins),
                signingCredentials: new SigningCredentials(
                    _securityKey,
                    SecurityAlgorithms.HmacSha256));

            return new JwtToken(token, _refreshToken, _systemIdentity, _creds, _tag);
        }

        public ClaimsPrincipal GetPrincipals(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters,
                out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        #region " private "

        private void EnsureArguments()
        {
            if (_securityKey == null)
                throw new ArgumentNullException("Security Key");

            if (string.IsNullOrEmpty(_issuer))
                throw new ArgumentNullException("Issuer");

            if (string.IsNullOrEmpty(_audience))
                throw new ArgumentNullException("Audience");
        }

        #endregion " private "
    }
}