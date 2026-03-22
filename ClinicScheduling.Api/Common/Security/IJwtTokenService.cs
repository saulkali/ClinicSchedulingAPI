namespace ClinicScheduling.Api.Common.Security;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string role);
}