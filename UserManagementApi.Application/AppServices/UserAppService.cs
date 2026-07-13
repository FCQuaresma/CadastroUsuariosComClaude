using UserManagementApi.Application.ViewModels;
using UserManagementApi.CrossCutting.Responses;
using UserManagementApi.Domain.Common;
using UserManagementApi.Domain.Entities;
using UserManagementApi.Services;

namespace UserManagementApi.Application.AppServices;

public class UserAppService
{
    private readonly UserService _userService;

    public UserAppService(UserService userService)
    {
        _userService = userService;
    }

    public async Task<ApiResponse<int>> CreateAsync(CreateUserViewModel viewModel)
    {
        var user = new User
        {
            Nome = viewModel.Nome,
            Email = viewModel.Email,
            Role = viewModel.Role
        };

        var id = await _userService.CreateAsync(user, viewModel.Senha);
        return ApiResponse<int>.Ok(id, "Usuário criado com sucesso.");
    }

    public async Task<ApiResponse<UserViewModel>> GetByIdAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return ApiResponse<UserViewModel>.Ok(ParaViewModel(user));
    }

    public async Task<ApiResponse<PagedResult<UserViewModel>>> GetPagedAsync(PagedRequest request)
    {
        var paged = await _userService.GetPagedAsync(request);

        var result = new PagedResult<UserViewModel>
        {
            Items = paged.Items.Select(ParaViewModel).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return ApiResponse<PagedResult<UserViewModel>>.Ok(result);
    }

    public async Task<ApiResponse<object?>> UpdateAsync(UpdateUserViewModel viewModel)
    {
        var userAtual = await _userService.GetByIdAsync(viewModel.Id);

        userAtual.Nome = viewModel.Nome;
        userAtual.Email = viewModel.Email;
        userAtual.Role = viewModel.Role;

        await _userService.UpdateAsync(userAtual);
        return ApiResponse<object?>.Ok(null, "Usuário atualizado com sucesso.");
    }

    public async Task<ApiResponse<object?>> InactivateAsync(int id)
    {
        await _userService.InactivateAsync(id);
        return ApiResponse<object?>.Ok(null, "Usuário inativado com sucesso.");
    }

    private static UserViewModel ParaViewModel(User user) =>
        new(user.Id, user.Nome, user.Email, user.Role, user.Ativo);
}
