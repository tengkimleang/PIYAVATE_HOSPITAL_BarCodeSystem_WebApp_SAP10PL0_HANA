using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Common.Interfaces.Setting;

public interface IJwtRegister
{
    Task<JwtResponse> GenerateToken(string account);
    Task<JwtResponse> GenerateRefreshToken();
}
