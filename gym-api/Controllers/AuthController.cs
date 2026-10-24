
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record LoginRequest(string Username, string Password);
public record RefreshRequest(string AccessToken, string RefreshToken);
public record CreateStaffRequest(string Username, string Password, string FullName);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    public AuthController(AppDbContext db, TokenService tokens) { _db = db; _tokens = tokens; }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        user.RefreshToken = _tokens.CreateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            accessToken = _tokens.CreateAccessToken(user, roles),
            refreshToken = user.RefreshToken,
            fullName = user.FullName,
            roles
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req)
    {
        var principal = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(req.AccessToken);
        var userId = int.Parse(principal.Claims
            .First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier
                     || c.Type == "nameid").Value);

        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.RefreshToken != req.RefreshToken
            || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        user.RefreshToken = _tokens.CreateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return Ok(new { accessToken = _tokens.CreateAccessToken(user, roles), refreshToken = user.RefreshToken });
    }

    [HttpPost("staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaff(CreateStaffRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

        var staff = new User
        {
            Username = req.Username,
            FullName = req.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        staff.UserRoles.Add(new UserRole { RoleId = 2 });
        _db.Users.Add(staff);
        await _db.SaveChangesAsync();
        return Ok(new { staff.Id, staff.Username, staff.FullName });
    }
}