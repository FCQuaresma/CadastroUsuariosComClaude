using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Application.AppServices;
using UserManagementApi.Application.ViewModels;

namespace UserManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _authAppService;

    public AuthController(AuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginViewModel viewModel)
    {
        var response = await _authAppService.LoginAsync(viewModel);
        return Ok(response);
    }
}
