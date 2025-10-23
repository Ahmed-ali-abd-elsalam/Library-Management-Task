using Application.IRepository;
using Application.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserTokenService :IUserTokenService
    {
        //private readonly ITokenRepository _repository;

        private readonly IConfiguration configuration;

        public UserTokenService(
            //ITokenRepository repository, 
            IConfiguration configuration)
        {
            //_repository = repository;
            this.configuration = configuration;
        }

        //public async Task<string> getUserTokenAsync(string Email ,string source)
        //{
        //    UserToken userToken = await _repository.GetUserTokenAsync(Email, source);
        //    if (userToken == null) return null;
        //    if (userToken.ExpiresAt <= DateTime.UtcNow)
        //    {
        //        _repository.DeleteUserToken(userToken);
        //        return null;
        //    }
        //    return userToken.token;
        //}
        //public async Task<UserToken> getTokenAsync(string token)
        //{
        //    UserToken userToken = await _repository.GetUserTokenAsync(token);
        //    if (userToken == null) return null;
        //    if (userToken.ExpiresAt <= DateTime.UtcNow)
        //    {
        //        _repository.DeleteUserToken(userToken);
        //        return null;
        //    }
        //    return userToken;
        //}
        //public async Task<bool> deleteTokenAsync(UserToken userToken)
        //{
        //   return await _repository.DeleteUserToken(userToken);
        //}

        public async Task<string> createTokenAsync(Member member, IList<string> roles, string mode, string source)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email , member.Email),
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["tokensecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken tokenDescriptor = null;
            if (mode == "Response Token")
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                tokenDescriptor = new JwtSecurityToken
               (
                   issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                   audience: configuration.GetValue<string>("AppSettings:Audience"),
                   claims: claims,
                   expires: DateTime.UtcNow.AddMinutes(30),
                   signingCredentials: creds
               );
            }
            else if (mode == "Refresh Token")
            {
                tokenDescriptor = new JwtSecurityToken
                (
                    issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                    audience: configuration.GetValue<string>("AppSettings:Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: creds
                );
            }
            //string Token = new JwtSecurityTokenHandler().ReadJwtToken();
            string Token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            //if(mode == "Response Token")
            //{
            //    UserToken userToken = new UserToken
            //    {
            //        token = Token,
            //        source = source,
            //        userEmail = member.Email,
            //        user = member,
            //        userId = member.Id,
            //        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            //    };
            //    await _repository.createUserTokenAsync(userToken);
            //}
            return Token;
        }
        public bool checkTokenValid(string token)
        {
            var endDate = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
            return DateTime.UtcNow < endDate;
        }

    }
}
