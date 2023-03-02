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
    public sealed class JwtTokenOPBuilder
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

        public JwtTokenOPBuilder(IOptions<JwtOnlinePayment> configuration)
        {
            _issuer = configuration.Value.Issuer;
            _audience = configuration.Value.Audience;
            _securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.Value.SecretKey));
            _expiryInMins = configuration.Value.TokenTimeout;
        }

        public JwtTokenOPBuilder AddSecurityKey(SecurityKey securityKey)
        {
            _securityKey = securityKey;
            return this;
        }

        public JwtTokenOPBuilder AddSubject(string subject)
        {
            _claims.Add(ClaimTypes.NameIdentifier, subject);
            return this;
        }

        public JwtTokenOPBuilder AddIssuer(string issuer)
        {
            _issuer = issuer;
            return this;
        }

        public JwtTokenOPBuilder AddAudience(string audience)
        {
            _audience = audience;
            return this;
        }

        public JwtTokenOPBuilder AddClaim(string type, string value)
        {
            if (!string.IsNullOrEmpty(value)) _claims.Add(type, value);
            return this;
        }

        public JwtTokenOPBuilder AddId(string value)
        {
            _claims.Add("ID", value);
            return this;
        }

        public JwtTokenOPBuilder AddTag(int value)
        {
            _tag = value;
            return this;
        }

        public JwtTokenOPBuilder AddCreds(long value)
        {
            _creds = value;
            return this;
        }

        public JwtTokenOPBuilder RemoveTag(string type)
        {
            _claims.Remove(type);
            return this;
        }

        public JwtTokenOPBuilder AddRole(Type type, string value)
        {
            _claims.Add(type.Name, value);
            return this;
        }

        public JwtTokenOPBuilder AddProductId(string productId)
        {
            _claims.Add("PID", productId);
            return this;
        }

        public JwtTokenOPBuilder AddClaims(Dictionary<string, string> claims)
        {
            _claims.Union(claims);
            return this;
        }

        public void AddMoneyTransAuthRole(Type type, string value)
        {
            var state = Enum.Parse(type, value);
            _claims.Add("MoneyTransferState", ((int)state).ToString());
        }

        public JwtTokenOPBuilder AddExpiryInMins(int expireyInMin)
        {
            _expiryInMins = expireyInMin;
            return this;
        }

        public JwtTokenOPBuilder AddRefreshToken(string refreshToken)
        {
            _refreshToken = refreshToken;
            return this;
        }

        public JwtTokenOPBuilder AddIdentity(string identity)
        {
            _systemIdentity = identity;
            return this;
        }

        public JwtToken Build(int transactionTime = -1)
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
                expires: DateTime.UtcNow.AddMinutes(transactionTime == -1 ? _expiryInMins : transactionTime),
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