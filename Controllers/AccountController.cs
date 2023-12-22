using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers;
public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountController(DataContext contexto, ITokenService tokenService){
        _context = contexto;
        _tokenService = tokenService;
    }

    [HttpPost("Register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username, 
            PassowordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
            passwordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

       return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user) 
        };
    } 

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> login(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => 
        x.UserName == loginDto.UserName);

        if(user == null) return Unauthorized("Invalid password");

        using var hmac = new HMACSHA512(user.passwordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if(user.PassowordHash == null) return Unauthorized("Invalid password");

        for(int i = 0; i <computedHash.Length; i++)
        {
            if(computedHash[i] != user.PassowordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user) 
        };

    }
    
    private async Task<bool> UserExists(string Username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == Username.ToLower()); 
    }
}