using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace WebDavFilesRepository.Server.Additions
{
    public static class Jwt
    {
        /// <summary>
        /// создает jwt токен
        /// </summary>
        /// <param name="claimsIdentity">требования для идентификации</param>
        /// <param name="issuer">издатель</param>
        /// <param name="audience">аудитория</param>
        /// <param name="key">секретный ключ</param>
        /// <returns>возвращает токен</returns>
        public static string CreateToken(ClaimsIdentity claimsIdentity, string issuer,
            string audience, TimeSpan lifeTime, string key)
        {
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claimsIdentity.Claims,
                    expires: DateTime.UtcNow.Add(lifeTime),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        /// <summary>
        /// проверить валидность токена
        /// </summary>
        /// <param name="token">токен</param>
        /// <param name="validationParameters">параметры для валидации</param>
        /// <returns>возвращает true в случае если токен валиден</returns>
        public static bool CheckToken(string token, TokenValidationParameters validationParameters)
        {
            JwtSecurityTokenHandler tokenHandler = new();

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// вытянуть identity из токена
        /// </summary>
        /// <param name="token">токен</param>
        /// <returns>возвращает описание о пользователе</returns>
        public static IIdentity? GetIdentityFromToken(string token)
        {
            JwtSecurityToken jwtToken = new(token);

            if (jwtToken == null) return null;

            // проверка токена на протухание
            if (jwtToken.ValidFrom > DateTime.UtcNow || jwtToken.ValidTo < DateTime.UtcNow)
            {
                // возвращаем не валидный identity
                return new ClaimsIdentity(jwtToken.Claims);
            }
            else
            {
                // возвращаем валидный identity
                return new ClaimsIdentity(jwtToken.Claims, "Authorize");
            }
        }

        /// <summary>
        /// сформировать настройки для валидации
        /// </summary>
        /// <param name="issuer">издатель</param>
        /// <param name="audience">аудитория</param>
        /// <param name="validateLifetime">проверять ли время жизни токена</param>
        /// <param name="key">ключ</param>
        /// <returns>возвращает сформированные параметры</returns>
        public static TokenValidationParameters GetValidationParameters(
            string issuer, string audience, bool validateLifetime, string key)
        {
            TokenValidationParameters response = new()
            {
                // указывает, будет ли валидироваться издатель при валидации токена
                ValidateIssuer = true,
                // строка, представляющая издателя
                ValidIssuer = issuer,
                // будет ли валидироваться потребитель токена
                ValidateAudience = true,
                // установка потребителя токена
                ValidAudience = audience,
                // будет ли валидироваться время существования
                ValidateLifetime = validateLifetime,
                // установка ключа безопасности
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
            };

            return response;
        }
    }
}