using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>JWT 生成、验证与 Cookie 读写</summary>
    public sealed class JwtTokenService : IDisposable
    {
        private const string RefreshTokenPath = "/api/SystemBasicMgmt/SystemAuth/SysUserOperate/RefreshToken";

        private readonly JwtSettings _settings;
        private readonly JwtSecurityTokenHandler _handler = new();
        private readonly ECDsa _ecdsaPrivate;
        private readonly ECDsa _ecdsaPublic;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;

            _ecdsaPrivate = CreateEcdsaFromPem(_settings.PrivateKey, "PrivateKey");

            // 未配置公钥时，直接用私钥验签
            _ecdsaPublic = string.IsNullOrWhiteSpace(_settings.PublicKey)
                ? CreateEcdsaFromPem(_settings.PrivateKey, "PrivateKey")
                : CreateEcdsaFromPem(_settings.PublicKey, "PublicKey");

            ValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = new ECDsaSecurityKey(_ecdsaPublic) { KeyId = _settings.KeyId },
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds),
                ValidAlgorithms = new[] { SecurityAlgorithms.EcdsaSha256 }
            };
        }

        /// <summary>验签参数，供 JwtBearer 与手动验证共用</summary>
        public TokenValidationParameters ValidationParameters { get; }

        /// <summary>Access Token 的 Cookie 名称</summary>
        public string CookieName =>
            string.IsNullOrWhiteSpace(_settings.CookieName) ? "AccessToken" : _settings.CookieName;

        /// <summary>Refresh Token 的 Cookie 名称</summary>
        public string RefreshCookieName =>
            string.IsNullOrWhiteSpace(_settings.RefreshCookieName) ? "RefreshToken" : _settings.RefreshCookieName;

        /// <summary>Refresh Token 有效期（天）</summary>
        public int RefreshTokenExpiresInDays => _settings.RefreshTokenExpiresInDays;

        /// <summary>登录成功：生成 Access Token 并写入 HttpOnly Cookie</summary>
        public void SetAuthCookie(HttpResponse response, long userId, string userNo)
        {
            ArgumentNullException.ThrowIfNull(response);

            var token = GenerateTokenString(userId, userNo);
            var expires = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiresInMinutes);

            response.Cookies.Append(CookieName, token, BuildCookieOptions(expires));
        }

        /// <summary>登出：清除 Access Token Cookie</summary>
        public void ClearAuthCookie(HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.Cookies.Delete(CookieName, BuildCookieOptions());
        }

        /// <summary>生成 Access Token 字符串</summary>
        public string GenerateTokenString(long userId, string userNo)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new(CurrentUser.ClaimUserId, userId.ToString()),
                new(CurrentUser.ClaimUserNo, userNo ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var signingKey = new ECDsaSecurityKey(_ecdsaPrivate) { KeyId = _settings.KeyId };
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_settings.ExpiresInMinutes),
                signingCredentials: credentials);

            return _handler.WriteToken(token);
        }

        /// <summary>生成 Refresh Token：明文写 Cookie，哈希入库</summary>
        public (string RawToken, string TokenHash) GenerateRefreshToken()
        {
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (raw, HashRefreshToken(raw));
        }

        /// <summary>Refresh Token 明文哈希（SHA256 Hex）</summary>
        public static string HashRefreshToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }

        /// <summary>写入 Refresh Token Cookie，仅在刷新接口路径下携带</summary>
        public void SetRefreshTokenCookie(HttpResponse response, string rawToken, DateTime expiresAt)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.Cookies.Append(RefreshCookieName, rawToken, BuildCookieOptions(expiresAt, RefreshTokenPath));
        }

        /// <summary>清除 Refresh Token Cookie</summary>
        public void ClearRefreshTokenCookie(HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.Cookies.Delete(RefreshCookieName, BuildCookieOptions(path: RefreshTokenPath));
        }

        /// <summary>手动验证 Token，失败返回 null</summary>
        public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
        {
            var parameters = ValidationParameters.Clone();
            parameters.ValidateLifetime = validateLifetime;

            try
            {
                return _handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        private CookieOptions BuildCookieOptions(DateTimeOffset? expires = null, string path = "/") => new()
        {
            HttpOnly = true,
            Secure = _settings.CookieSecure,
            SameSite = _settings.CookieSameSite,
            Expires = expires,
            Path = path
        };

        /// <summary>从 PEM 创建 ECDSA，兼容 JSON 中的字面 \n</summary>
        private static ECDsa CreateEcdsaFromPem(string pem, string keyName)
        {
            var ecdsa = ECDsa.Create();

            try
            {
                ecdsa.ImportFromPem(pem.Replace("\\n", "\n").Trim());
            }
            catch (CryptographicException ex)
            {
                ecdsa.Dispose();
                throw new CryptographicException($"导入 ECDSA {keyName} 失败，请检查 PEM 格式。", ex);
            }

            return ecdsa;
        }

        public void Dispose()
        {
            _ecdsaPrivate.Dispose();
            _ecdsaPublic.Dispose();
        }
    }
}
