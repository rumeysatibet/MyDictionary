using Microsoft.AspNetCore.Mvc;
using MyDictionary.ApiService.DTOs;
using MyDictionary.ApiService.Services;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Geçersiz veri girişi."
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var result = await _authService.RegisterAsync(registerDto, ipAddress);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Geçersiz veri girişi."
            });
        }

        var result = await _authService.LoginAsync(loginDto);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }


    [HttpGet("user-agreement")]
    public async Task<ActionResult<UserAgreementDto>> GetUserAgreement()
    {
        var agreement = await _authService.GetActiveUserAgreementAsync();
        
        if (agreement == null)
        {
            return NotFound(new { Message = "Aktif kullanıcı sözleşmesi bulunamadı." });
        }
        
        return Ok(agreement);
    }

    [HttpPost("validate-token")]
    public async Task<ActionResult<UserDto>> ValidateToken([FromBody] TokenValidationDto tokenDto)
    {
        if (string.IsNullOrEmpty(tokenDto.Token))
        {
            return BadRequest(new { Message = "Token gerekli." });
        }

        var user = await _authService.ValidateTokenAsync(tokenDto.Token);
        
        if (user == null)
        {
            return Unauthorized(new { Message = "Geçersiz veya süresi dolmuş token." });
        }
        
        return Ok(user);
    }

    [HttpGet("debug/users")]
    public async Task<ActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }
}