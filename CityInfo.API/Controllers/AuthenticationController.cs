﻿using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ICityInfoRepository cityInfoRepository;
        private readonly IConfiguration configuration;
        public AuthenticationController(ICityInfoRepository cityInfoRepository, IConfiguration configuration)
        {
            this.cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = await cityInfoRepository.GetUserAsync(authenticationRequestBody);
            if (user is null)
                return Unauthorized();

            var secret = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authentication:Secret"]));
            var signingCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            var claimsForToken = new List<Claim>()
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("given_name", user.FirstName ?? "Not provided"),
                new Claim("family_name", user.LastName ?? "Not provided")
            };
            var token = new JwtSecurityToken
            (
                configuration["Authentication:Issuer"],
                configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                signingCredentials
            );
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { username = user.Username, token = tokenToReturn });
        }

        [HttpPost("signup")]
        public async Task<ActionResult<string>> SignUp(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = await cityInfoRepository.RegisterUserAsync(authenticationRequestBody);
            if (user is null)
                return BadRequest();

            return await Login(authenticationRequestBody);
        }
    }
}
