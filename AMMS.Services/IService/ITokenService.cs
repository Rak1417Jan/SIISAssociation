using MVEA.Model.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Services.IService
{
    public interface ITokenService
    {
        string GenerateJwtToken(LoginResponse user);
        string GenerateJwtToken(string mobileNo, int? memberId = null);
        string GenerateRefreshToken();
    }
}
