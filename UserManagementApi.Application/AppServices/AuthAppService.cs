using UserManagementApi.Application.ViewModels;
using UserManagementApi.CrossCutting.Auth;
using UserManagementApi.CrossCutting.Responses;
using UserManagementApi.Services;

namespace UserManagementApi.Application.AppServices;

public class AuthAppService
{
    private readonly UserService _userService;
    private readonly ITokenService _tokenService;

    public AuthAppService(UserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginViewModel viewModel)
    {
        var user = await _userService.ValidateCredentialsAsync(viewModel.Email, viewModel.Senha);
        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);
        return ApiResponse<string>.Ok(token, "Login realizado com sucesso.");
    }
}
