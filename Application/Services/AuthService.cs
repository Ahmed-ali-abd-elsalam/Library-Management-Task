using Application.DTOs;
using Application.IService;
using Application.Mappers;
using Application.Results;
using Domain.Entities;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Services
{
    enum tokenModes
    {
        EmailValidation,
        PasswordReset
    }
    public class AuthService : IAuthService
    {
        private readonly UserManager<Member> _userManager;
        private readonly IMemberService memberService;
        private readonly IUserTokenService tokenService;
        private readonly IDistributedCache cache;
        private readonly IFluentEmail fluentEmail;
        private readonly IConfirmationTokenService ConfirmationTokenService;
        private readonly LinkFactory linkFactory;

        public AuthService(UserManager<Member> userManager,
            IUserTokenService tokenService,
            IDistributedCache cache,
            IFluentEmail fluentEmail,
            IConfirmationTokenService ConfirmEmailService,
            LinkFactory linkFactory,
            IMemberService memberService)
        {
            _userManager = userManager;
            this.tokenService = tokenService;
            this.cache = cache;
            this.fluentEmail = fluentEmail;
            this.ConfirmationTokenService = ConfirmEmailService;
            this.linkFactory = linkFactory;
            this.memberService = memberService;
        }

        public async Task<Result<LoginResponseDto>> Login(LoginMemberDto loginMemberDto, string source, CancellationToken cancellationToken)
        {
            Member user = await _userManager.FindByEmailAsync(loginMemberDto.Email);
            if (user == null) return Errors.DoesntExist;
            bool result = await _userManager.CheckPasswordAsync(user, loginMemberDto.Password);
            if (result == false) return Errors.WrongPassword;
            if (!user.EmailConfirmed) return Errors.EmailNotConfirmed;
            var userRoles = await _userManager.GetRolesAsync(user);
            string key = $"{loginMemberDto.Email}-{source}";
            string ResponseToken = await cache.GetStringAsync(key, cancellationToken);
            string Refresh_token = await tokenService.createTokenAsync(user, userRoles, "Refresh Token", source);
            user.RefreshToken = Refresh_token;
            await memberService.editMember(loginMemberDto.Email, user);
            if (ResponseToken is not null)
            {
                return new LoginResponseDto
                {
                    Email = loginMemberDto.Email,
                    Response_Token = ResponseToken,
                    Refresh_token = Refresh_token
                };
            }
            ResponseToken = await tokenService.createTokenAsync(user, userRoles, "Response Token", source);
            await cache.SetStringAsync(
                key,
                ResponseToken,
                new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)),
                cancellationToken);
            return new LoginResponseDto
                {
                    Email = loginMemberDto.Email,
                    Response_Token = ResponseToken,
                    Refresh_token = Refresh_token
                };

        }
        public async Task<Result<LoginResponseDto>> refresh(string userEmail, string RefreshToken, string source, CancellationToken cancellationToken)
        {
            Member user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return Errors.DoesntExist;
            if (user.RefreshToken != RefreshToken || !tokenService.checkTokenValid(user.RefreshToken))
            {
                return Errors.RefreshToken;
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            string key = $"{userEmail}-{source}";
            string ResponseToken = await cache.GetStringAsync(key, cancellationToken);
            if (ResponseToken is not null)
            {
                return new LoginResponseDto
{
    Email = userEmail,
    Response_Token = ResponseToken,
    Refresh_token = RefreshToken
};
            }
            ResponseToken = await tokenService.createTokenAsync(user, userRoles, "Response Token", source);
            await cache.SetStringAsync(
                key,
                ResponseToken,
                new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30)),
                cancellationToken);
                return new LoginResponseDto

            {
                Email = userEmail,
                Response_Token = ResponseToken,
                Refresh_token = RefreshToken
            };
        }

        public async Task<Result<MemberResponseDto>> Signup(RegisterMemberDto registerMemberDto)
        {

            Member user = await _userManager.FindByEmailAsync(registerMemberDto.Email);
            if (user != null) return Errors.EmailTaken;
            Member member = registerMemberDto.RegisterDtoToMember();
            var result = await _userManager.CreateAsync(member, registerMemberDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(member, "Member");
                ConfirmationToken confirmationToken = await ConfirmationTokenService.generateTokenAsync(member.Email, tokenModes.EmailValidation.ToString());
                string link = linkFactory.generateLink(tokenModes.EmailValidation.ToString(), member.Email, confirmationToken.id.ToString());
                fluentEmail
                    .To(member.Email)
                    .Subject("Email Confirmation")
                    .Body($"To Validate Email <a href=\"{link}\">click here</a>", isHtml: true)
                    .SendAsync();
                return member.ToMemberResponseDto();
            }
            else
            {
                Console.WriteLine(result.Errors);
                return Errors.PasswordNotSecure;
            }
        }
        public async Task<Result> confirmEmail(string Email, string TokenId)
        {
            Member? user = await _userManager.FindByEmailAsync(Email);
            if (user is null) return Errors.DoesntExist;
            bool validateToken = await ConfirmationTokenService.ValidateTokenAsync(Guid.Parse(TokenId), tokenModes.EmailValidation.ToString(), Email);
            if (!validateToken) return Errors.InvalidToken;
            user.EmailConfirmed = true;
            return await memberService.editMember(Email, user)? Result.success():Errors.DoesntExist;

        }
        public async Task<Result> logOutAsync(string email, string source, CancellationToken cancellationToken)
        {
            Member user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Errors.DoesntExist;
            string key = $"{email}-{source}";
            await cache.RemoveAsync(key, cancellationToken);
            return Result.success();
        }

        public async Task<Result> resetPasswordInitializeAsync(string email)
        {
            Member user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Errors.DoesntExist;
            ConfirmationToken confirmationToken = await ConfirmationTokenService.generateTokenAsync(email, tokenModes.PasswordReset.ToString());
            string link = linkFactory.generateLink(tokenModes.PasswordReset.ToString(), email, confirmationToken.id.ToString());
            fluentEmail
                .To(email)
                .Subject("Password Reset")
                .Body($"To rest your password <a href=\"{link}\">click here</a>", isHtml: true)
                .SendAsync();
            return Result.success();
        }


        public async Task<Result> resetPassword(ForgotPasswrodDTO forgotPasswrodDTO, string TokenId, string Email)
        {
            Member? user = await _userManager.FindByEmailAsync(Email);
            if (user is null) return Errors.DoesntExist;
            bool validateToken = await ConfirmationTokenService.ValidateTokenAsync(Guid.Parse(TokenId), tokenModes.PasswordReset.ToString(), Email);
            if (!validateToken) return Errors.InvalidToken;
            if (!forgotPasswrodDTO.NewPassword.Equals(forgotPasswrodDTO.ConfirmNewPassword)) return Errors.WrongPassword;
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, forgotPasswrodDTO.NewPassword);
            if (!result.Succeeded)
            {
                Console.WriteLine(result.Errors.ToList());
                return Errors.PasswordNotSecure;
            }
            return Result.success();
        }
    }
}
