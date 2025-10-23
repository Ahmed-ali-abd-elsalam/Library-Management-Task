using Application.DTOs;
using Application.IService;
using Application.Mappers;
using Application.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Controller]
    [Route("/api/[Controller]")]
    public class AuthController :ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginMemberDto loginMemberDto,CancellationToken cancellationToken) {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            string source = HttpContext.Request.Headers["User-Agent"];
            var memberResponseResult = await authService.Login(loginMemberDto,source,cancellationToken);
            if (!memberResponseResult.IsSuccess)
            {
                if (memberResponseResult.error == Errors.DoesntExist) return NotFound(memberResponseResult);
                else return BadRequest(memberResponseResult);
            }
                return Ok(memberResponseResult);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterMemberDto registerMemberDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var memberResponse = await authService.Signup(registerMemberDto);
            if (!memberResponse.IsSuccess) return BadRequest(memberResponse);
            return Ok(memberResponse);
        }

        [HttpDelete]
        [Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            string email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return BadRequest("Invalid Token");
            string source= HttpContext.Request.Headers["User-Agent"];
            var result = await authService.logOutAsync(email,source ,cancellationToken);
            if (!result.IsSuccess) return NotFound(result);
            return Ok("Logged Out Successfully");
        }

        [HttpPost]
        [Authorize]
        [Route("refresh")]
        public async Task<IActionResult> refresh(CancellationToken cancellationToken)
        {
            string source = HttpContext.Request.Headers["User-Agent"];
            var user = User.Claims.FirstOrDefault(ct => ct.Type == ClaimTypes.Email);
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            string token = authHeader.Substring("Bearer ".Length).Trim();
            var ResponseDto = await authService.refresh(user.Value,token,source,cancellationToken);
            if (!ResponseDto.IsSuccess & ResponseDto.error == Errors.RefreshToken) return BadRequest(ResponseDto);
            return Ok(ResponseDto);
        }
        
        [HttpGet]
        [Route("forget-Password-start")]
        public async Task<IActionResult> resetPasswordToken(string Email)
        {
            Result result = await authService.resetPasswordInitializeAsync(Email);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok("to Reset your password check your email");
        }

        [HttpPut]
        [Route("forget-password")]

        public async Task<IActionResult> resetpassword(string TokenId,string Email,[FromBody]ForgotPasswrodDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            Result result = await authService.resetPassword(forgotPasswordDTO,TokenId,Email);
            if (!result.IsSuccess) return BadRequest(result);
            return Accepted();
        }

        [HttpGet]
        [Route("confirm-email")]
        public async Task<IActionResult> confirmemail(string Email,string TokenId)
        {
            var result = await authService.confirmEmail(Email, TokenId);
            if (!result.IsSuccess) return BadRequest(result);
            return Accepted("Email Validated");
        }

        //[HttpGet]
        //public async Task<IActionResult> SendEmail(string Email)
        //{
        //    throw new Exception("test");
        //    await authService.SendEmail(Email);
        //    return Ok("Email Has been sent");
        //}
    }
}
