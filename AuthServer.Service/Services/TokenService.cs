using AuthServer.Core.Configuration;
using AuthServer.Core.Dto;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class TokenService : ITokenService
    {

        private readonly UserManager<UserApp> _userManager;
        private readonly CustomTokenOptions _tokenOption;

        public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOptions> tokenOption)
        {
            _userManager = userManager;
            _tokenOption = tokenOption.Value;
        }

        //32 bytlelik random bir değer üretir. Neredeyse tekrar üretme olasılığı trilyonda 1 dir.
        private string CreateRefreshToken() 
        
        {
            var numberByte = new Byte[32];
            using var random = RandomNumberGenerator.Create();

            random.GetBytes(numberByte);

            return Convert.ToBase64String(numberByte);
        }

        private IEnumerable<Claim> GetClaim(UserApp userapp,List<String> audiences) 
        {
            var userList = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier,userapp.Id),
        new Claim(JwtRegisteredClaimNames.Email,userapp.Email),
        new Claim(ClaimTypes.Name,userapp.UserName),
        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };
            userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud,x)));
            return userList;
        }

        private IEnumerable<Claim> GetClaimsByClient(Client client) 
        {
        var claims = new List<Claim>();
        claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString());

            return claims;
        }

        public TokenDto CreateToken(UserApp userapp)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            var RefreshTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);

            var securityKey = SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);

            SigningCredentials signingCredentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecuritytoken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: GetClaim(userapp, _tokenOption.Audience),
                signingCredentials: signingCredentials
                );

            var handler = new JwtSecurityTokenHandler();

            var token = handler.WriteToken(jwtSecuritytoken);

            var tokenDto = new TokenDto 
            { AccessToken = token,
            RefreshToken = CreateRefreshToken(),
            AccesTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = RefreshTokenExpiration
            };

            return tokenDto;
        }

        public ClientTokenDto CreateTokenByClient(Client client)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);


            var securityKey = SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);

            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecuritytoken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims:GetClaimsByClient(client),
                signingCredentials: signingCredentials
                );

            var handler = new JwtSecurityTokenHandler();

            var token = handler.WriteToken(jwtSecuritytoken);

            var tokenDto = new ClientTokenDto
            {
                AccessToken = token,
                AccesTokenExpiration = accessTokenExpiration,
            };

            return tokenDto;
        }
    }
}
