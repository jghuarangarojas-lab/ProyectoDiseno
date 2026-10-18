using Auth.Application.Common.Models;
using Auth.Domain.Entities;

namespace Auth.Application.Common.Interfaces;

public interface IJwtProvider
{
    JwtTokenResult GenerateToken(User user);
}
