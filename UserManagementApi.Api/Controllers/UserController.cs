using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Application.AppServices;
using UserManagementApi.Application.ViewModels;
using UserManagementApi.CrossCutting.Responses;
using UserManagementApi.Domain.Common;

namespace UserManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserAppService _userAppService;

    public UserController(UserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync(CreateUserViewModel viewModel)
    {
        var response = await _userAppService.CreateAsync(viewModel);
        return CreatedAtRoute("GetUserById", new { id = response.Data }, response);
    }

    [HttpGet("{id:int}", Name = "GetUserById")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var response = await _userAppService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] PagedRequest request)
    {
        var response = await _userAppService.GetPagedAsync(request);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync(int id, UpdateUserViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest(ApiResponse<object?>.Fail("O Id da rota não corresponde ao Id do corpo da requisição."));
        }

        var response = await _userAppService.UpdateAsync(viewModel);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InactivateAsync(int id)
    {
        var response = await _userAppService.InactivateAsync(id);
        return Ok(response);
    }
}
