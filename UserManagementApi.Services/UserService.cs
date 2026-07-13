using FluentValidation;
using UserManagementApi.CrossCutting.Exceptions;
using UserManagementApi.Domain.Common;
using UserManagementApi.Domain.Entities;
using UserManagementApi.Domain.Repositories;

namespace UserManagementApi.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<User> _validator;

    public UserService(IUserRepository userRepository, IValidator<User> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<int> CreateAsync(User user, string senha)
    {
        await ValidarAsync(user);

        var existente = await _userRepository.GetByEmailAsync(user.Email);
        if (existente is not null)
        {
            throw new BusinessException($"Já existe um usuário cadastrado com o e-mail '{user.Email}'.");
        }

        user.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        user.Ativo = true;

        return await _userRepository.CreateAsync(user);
    }

    public async Task<User> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user ?? throw new NotFoundException($"Usuário com Id {id} não encontrado.");
    }

    public async Task<PagedResult<User>> GetPagedAsync(PagedRequest request)
    {
        return await _userRepository.GetPagedAsync(request);
    }

    public async Task UpdateAsync(User user)
    {
        await ValidarAsync(user);
        await GetByIdAsync(user.Id);

        var comMesmoEmail = await _userRepository.GetByEmailAsync(user.Email);
        if (comMesmoEmail is not null && comMesmoEmail.Id != user.Id)
        {
            throw new BusinessException($"Já existe um usuário cadastrado com o e-mail '{user.Email}'.");
        }

        await _userRepository.UpdateAsync(user);
    }

    public async Task InactivateAsync(int id)
    {
        await GetByIdAsync(id);
        await _userRepository.InactivateAsync(id);
    }

    public async Task<User> ValidateCredentialsAsync(string email, string senha)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || !user.Ativo || !BCrypt.Net.BCrypt.Verify(senha, user.SenhaHash))
        {
            throw new BusinessException("E-mail ou senha inválidos.");
        }

        return user;
    }

    private async Task ValidarAsync(User user)
    {
        var resultado = await _validator.ValidateAsync(user);
        if (!resultado.IsValid)
        {
            var mensagens = string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage));
            throw new BusinessException(mensagens);
        }
    }
}
