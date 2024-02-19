using AuthServer.Core.Dto;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TokenBazliKimlikDogrulama.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService) 
        {
        _authenticationService = authenticationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto) 
        {
            var result = await _authenticationService.CreateTokenAsync(loginDto);

            if(result.StatusCode == 200) 
            {
                return Ok(result);
            }
            else return BadRequest();
        }

        [HttpPost]

        public async Task <IActionResult> CreateTokenByClient(ClientLoginDto clientLoginDto) 
        {
            var result = _authenticationService.CreateTokenByClient(clientLoginDto);

            if (result.StatusCode == 200)
            {
                return Ok(result);
            }
            else return BadRequest();
        }


    }
}
