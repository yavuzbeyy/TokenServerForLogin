using AuthServer.Core.Configuration;
using AuthServer.Core.Dto;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {

        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenService;

        public AuthenticationService(IOptions<List<Client>> clients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenService)
        {
            _clients = clients.Value;
            _tokenService = tokenService;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            _userRefreshTokenService = userRefreshTokenService;
        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if(loginDto == null) throw new ArgumentNullException(nameof(loginDto));

            var user = await userManager.FindByEmailAsync(loginDto.Email);

            if(user==null) 
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400,true); 
            }

            if(!await userManager.CheckPasswordAsync(user,password:loginDto.Password)) 
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }

            var token = _tokenService.CreateToken(user);

            var userRefreshToken =  _userRefreshTokenService.Where(x=>x.UserId == user.Id).SingleOrDefault();

            if(userRefreshToken == null) 
            {
            await _userRefreshTokenService.AddAsync(new UserRefreshToken { UserId = user.Id,Code = token.RefreshToken,Expiration = token.RefreshTokenExpiration });
            }
            else 
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token,200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientloginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientloginDto.ClientId && x.Secret == clientloginDto.ClientSecret);

            if (client == null) 
            {
                return Response<ClientTokenDto>.Fail("ClientId or ClientSecret Not Found",404,true);
            }
            var token = _tokenService.CreateTokenByClient(client);

            return Response<ClientTokenDto>.Success(token,200);

        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefault();

            if(existRefreshToken== null) 
            {
                return Response<TokenDto>.Fail("RefreshTokenNotFound", 404, true);
            }

            var user = await userManager.FindByIdAsync(existRefreshToken.UserId);

            if(user==null) 
            {
            return Response<TokenDto>.Fail("User Id not Found",404,true);
            }

            var tokenDto = _tokenService.CreateToken(user);

            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

            await unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto,200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken = _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefault();

            if (existRefreshToken== null) 
            {
                return Response<NoDataDto>.Fail("RefreshTokenNotFound", 404, true);
            }

            _userRefreshTokenService.Remove(existRefreshToken);
            await unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
