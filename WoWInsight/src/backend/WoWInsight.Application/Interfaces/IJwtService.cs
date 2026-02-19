using System;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserAccount user);
    string GenerateRefreshToken();
}
