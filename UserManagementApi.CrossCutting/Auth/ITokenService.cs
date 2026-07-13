namespace UserManagementApi.CrossCutting.Auth;

public interface ITokenService
{
    string GenerateToken(int userId, string email, string role);
}
