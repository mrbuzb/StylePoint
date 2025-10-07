using FluentEmail.Core;
using FluentEmail.Smtp;
using FluentValidation;
using Google.Apis.Auth;
using StylePoint.Application.Dtos;
using StylePoint.Application.Helpers;
using StylePoint.Application.Helpers.Security;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Core.Errors;
using StylePoint.Domain.Entities;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace StylePoint.Application.Services.Implementations;

public class AuthService(IRoleRepository _roleRepo, IValidator<UserCreateDto> _validator,
    IUserRepository _userRepo, ITokenService _tokenService, IValidator<UserLoginDto> _validatorForLogin,
    IRefreshTokenRepository _refTokRepo) : IAuthService
{


    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleAuthDto dto)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings());

        var user = await _userRepo.GetUserByGoogleId(payload.Subject);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var userTokenDto = new UserGetDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Confirmer!.Email,
            Role = user.Role.Name
        };

        var token = _tokenService.GenerateToken(userTokenDto);

        var loginResponseDto = new LoginResponseDto
        {
            AccessToken = token,
        };

        return loginResponseDto;
    }



    public async Task<long> GoogleRegisterAsync(GoogleAuthDto dto)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings());

        var user = await _userRepo.GetUserByGoogleId(payload.Subject);

        var userByEmail = await _userRepo.GetUserByEmailAsync(payload.Email);

        if (user != null || userByEmail != null)
        {
            return user.UserId;
        }

        user = new User
        {
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            GoogleId = payload.Subject,
            ProfileImgUrl = payload.Picture,
            RoleId = await _roleRepo.GetRoleIdAsync("User")
        };

        var userId = await _userRepo.AddUserAsync(user);

        var userEntity = await _userRepo.GetUserByIdAsync(userId);

        userEntity.Confirmer = new UserConfirme
        {
            UserId = userId,
            Email = payload.Email,
            IsConfirmed = payload.EmailVerified,
        };

        userEntity.Card = new Card
        {
            Balance = 0,
            CardNumber = Guid.NewGuid(),
            UserId = userId
        };

        await _userRepo.UpdateUserAsync(userEntity);
        return user.UserId;
    }



    public async Task<long> SignUpUserAsync(UserCreateDto userCreateDto)
    {
        var validatorResult = await _validator.ValidateAsync(userCreateDto);
        if (!validatorResult.IsValid)
        {
            string errorMessages = string.Join("; ", validatorResult.Errors.Select(e => e.ErrorMessage));
            throw new AuthException(errorMessages);
        }

        var isEmailExists = await _userRepo.GetUserByEmailAsync(userCreateDto.Email);

        if (isEmailExists == null)
        {

            var tupleFromHasher = PasswordHasher.Hasher(userCreateDto.Password);

            var confirmer = new UserConfirme()
            {
                IsConfirmed = false,
                Email = userCreateDto.Email,
            };


            var user = new User()
            {
                Confirmer = confirmer,
                Password = tupleFromHasher.Hash,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                Salt = tupleFromHasher.Salt,
                RoleId = await _roleRepo.GetRoleIdAsync("User")
            };

            long userId = await _userRepo.AddUserAsync(user);

            var foundUser = await _userRepo.GetUserByIdAsync(userId);

            foundUser.Confirmer!.UserId = userId;


            foundUser.Card = new Card
            {
                Balance = 0,
                CardNumber = Guid.NewGuid(),
                UserId = userId
            };

            await _userRepo.UpdateUserAsync(foundUser);

            return userId;
        }
        else if (isEmailExists.Confirmer!.IsConfirmed is false)
        {

            var tupleFromHasher = PasswordHasher.Hasher(userCreateDto.Password);

            isEmailExists.FirstName = userCreateDto.FirstName;
            isEmailExists.LastName = userCreateDto.LastName;
            isEmailExists.Password = tupleFromHasher.Hash;
            isEmailExists.Salt = tupleFromHasher.Salt;
            isEmailExists.RoleId = await _roleRepo.GetRoleIdAsync("User");

            await _userRepo.UpdateUserAsync(isEmailExists);
            return isEmailExists.UserId;
        }

        throw new NotAllowedException("This email already confirmed");
    }


    public async Task<LoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto)
    {
        var resultOfValidator = _validatorForLogin.Validate(userLoginDto);
        if (!resultOfValidator.IsValid)
        {
            string errorMessages = string.Join("; ", resultOfValidator.Errors.Select(e => e.ErrorMessage));
            throw new AuthException(errorMessages);
        }

        var user = await _userRepo.GetUserByEmailAsync(userLoginDto.Email);

        if(user is null)
        {
            throw new UnauthorizedException($"User not found with email {userLoginDto.Email}");
        }

        var checkUserPassword = PasswordHasher.Verify(userLoginDto.Password, user.Password, user.Salt);

        if (checkUserPassword == false)
        {
            throw new UnauthorizedException("UserName or password incorrect");
        }
        if (user.Confirmer.IsConfirmed == false)
        {
            throw new UnauthorizedException("Email not confirmed");
        }

        var userGetDto = new UserGetDto()
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Confirmer.Email,
            Role = user.Role.Name,
        };

        var token = _tokenService.GenerateToken(userGetDto);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenToDB = new RefreshToken()
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(21),
            IsRevoked = false,
            UserId = user.UserId
        };

        await _refTokRepo.AddRefreshToken(refreshTokenToDB);

        var loginResponseDto = new LoginResponseDto()
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            User = new UserGetDto { Email = user.Confirmer.Email, ProfileImgUrl = user.ProfileImgUrl, Role = user.Role.Name, UserId = user.UserId, FirstName = user.FirstName,LastName = user.LastName },
            TokenType = "Bearer",
            Expires = 24
        };


        return loginResponseDto;
    }

    public async Task ForgotPassword(string email, string newPassword, string confirmCode)
    {
        bool isValid = System.Text.RegularExpressions.Regex.IsMatch(email, @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");

        if (!isValid)
        {
            throw new NotAllowedException();
        }
        var user = await _userRepo.GetUserByEmailAsync(email);
        if(user == null)
        {
            throw new EntityNotFoundException("User not found");
        }
        var code = user.Confirmer!.ConfirmingCode;
        if (code == null || code != confirmCode || user.Confirmer.ExpiredDate < DateTime.Now)
        {
            throw new Exception("Code is incorrect");
        }

        var taple = PasswordHasher.Hasher(newPassword);

        user.Password = taple.Hash;
        user.Salt = taple.Salt;

        await _userRepo.UpdateUserAsync(user);
    }



    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshRequestDto request)
    {
        ClaimsPrincipal? principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null) throw new ForbiddenException("Invalid access token.");


        var userClaim = principal.FindFirst(c => c.Type == "UserId");
        var userId = long.Parse(userClaim.Value);


        var refreshToken = await _refTokRepo.SelectRefreshToken(request.RefreshToken, userId);
        if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow || refreshToken.IsRevoked)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        refreshToken.IsRevoked = true;

        var user = await _userRepo.GetUserByIdAsync(userId);

        var userGetDto = new UserGetDto()
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Confirmer!.Email,
            Role = user.Role.Name,
        };

        var newAccessToken = _tokenService.GenerateToken(userGetDto);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenToDB = new RefreshToken()
        {
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(21),
            IsRevoked = false,
            UserId = user.UserId
        };

        await _refTokRepo.AddRefreshToken(refreshTokenToDB);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenType = "Bearer",
            Expires = 24
        };
    }

    public async Task LogOut(string token) => await _refTokRepo.DeleteRefreshToken(token);

    public async Task EailCodeSender(string email)
    {
        var user = await _userRepo.GetUserByEmailAsync(email);

        var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Port = 587,
            Credentials = new NetworkCredential("qahmadjon11@gmail.com", "nhksnhhxzdbbnqdw")
        });

        Email.DefaultSender = sender;

        var code = Random.Shared.Next(100000, 999999).ToString();

        var sendResponse = await Email
            .From("qahmadjon11@gmail.com")
            .To(email)
            .Subject("Your Confirming Code")
            .Body(code)
            .SendAsync();

        user.Confirmer!.ConfirmingCode = code;
        user.Confirmer.ExpiredDate = DateTime.UtcNow.AddHours(5).AddMinutes(10);
        await _userRepo.UpdateUserAsync(user);
    }

    public async Task<bool> ConfirmCode(string userCode, string email)
    {
        var user = await _userRepo.GetUserByEmailAsync(email);
        var code = user.Confirmer!.ConfirmingCode;
        if (code == null || code != userCode || user.Confirmer.ExpiredDate < DateTime.Now || user.Confirmer.IsConfirmed is true)
        {
            throw new NotAllowedException("Code is incorrect");
        }
        user.Confirmer.IsConfirmed = true;
        await _userRepo.UpdateUserAsync(user);
        return true;
    }
}
