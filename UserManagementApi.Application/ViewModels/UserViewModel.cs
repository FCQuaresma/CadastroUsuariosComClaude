namespace UserManagementApi.Application.ViewModels;

public record UserViewModel(int Id, string Nome, string Email, string Role, bool Ativo);
