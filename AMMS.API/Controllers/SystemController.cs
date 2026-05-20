using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;


namespace MVEA.API.Controllers;

[ApiController]
//[ApiVersion("1.0")]
//[Route("api/v{version:apiVersion}/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public SystemController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    

   
}
