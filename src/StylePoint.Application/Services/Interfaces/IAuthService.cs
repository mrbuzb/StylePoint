using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IAuthService
{
    Task<long> SignUpUserAsync(UserCreateDto userCreateDto);
    Task<LoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshRequestDto request);
    Task EailCodeSender(string email);
    Task LogOut(string token);
    Task ForgotPassword(string email, string newPassword, string confirmCode);
    Task<bool> ConfirmCode(string userCode, string email);
    Task<long> GoogleRegisterAsync(GoogleAuthDto dto);
    Task<LoginResponseDto> GoogleLoginAsync(GoogleAuthDto dto);
}