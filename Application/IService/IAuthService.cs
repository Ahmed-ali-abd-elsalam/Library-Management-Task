using Application.DTOs;
using Application.Results;

namespace Application.IService
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> refresh(string userEmail, string refreshToken, string source, CancellationToken cancellationToken);
        Task<Result<LoginResponseDto>> Login(LoginMemberDto loginMemberDto, string source, CancellationToken cancellationToken);
        Task<Result<MemberResponseDto>> Signup(RegisterMemberDto registerMemberDto);
        public Task<Result> logOutAsync(string email, string source, CancellationToken cancellationToken);
        public Task<Result> resetPassword(ForgotPasswrodDTO forgetPasswrodDTO, string TokenId, string Email);
        public Task<Result> confirmEmail(string Email, string Token);
        public Task<Result> resetPasswordInitializeAsync(string email);

    }
}
