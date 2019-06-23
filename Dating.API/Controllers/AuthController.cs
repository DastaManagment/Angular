using Dating.API.Data;
using Dating.API.Dtos;
using Dating.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dating.API.Controllers
{
    [Route("api/[controller]")]
    //ამის დაწერა კონტროლერის დასაწყისშივე ცონტორლერის მეთოდებს აძლევს საშუალებას რო defaul-თად მნიშვნელობები აიღოს ბოდიდან თუ ეს არ გვექნება
    //მაშინ უნდა მიეთითოს რომელი ნაწილიდან აიღოს მეთოდმა პარამეტრები
    //თუ იყენებ ApiControllers ასევე არ გვჭირდება მოდელსთეითობის შედარება ამას თვითონ აკეთებს
    [ApiController]
    public class AuthController : ControllerBase
    {
        //ინტერფეისის კლასისპროპეთის აღწერა
        private readonly IAuthRepository _repo;
        //საჭიროა რადგან შესაძლებელი იყოს  ფაილიდან ინფორმმაციის აღება სექციის გადაწოდებით მაგ: login ფუნქციაში
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username))
                //ბედრექვესთი მხოლოდ იმ შემთხვევაშა წვდოომადი თუ ცონტორლერი გამოცხადებულია ControllerBase კლასის შვილობილად
                return BadRequest("Username Already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            //succsesful რესპონსი
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginForDto userLoginForDto)
        {
            var userFromRepo = await _repo.Login(userLoginForDto.Username.ToLower(), userLoginForDto.Password);

            if (userFromRepo == null)
                return Unauthorized();
            //tokeni რომელიც შეიცავს ინფორმაციას უსერის აიდზე და სახელზე 
            //რის უფლებას გვაძლევს თოკენი : თოკენი შეიძლება იყოს ვალიდური სერვერის მხრიდან 
            //და არ ხდება საჭირო ყოველ ჯერზე ბაზიდან წამოღება უსერნეიმის და აიდის
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.ID.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)

            };

            //ტოკენი უნდა დაიფაროს რადგან არ იყოს კითხვადი
            //token-ი დაგენერირდა app-setting ფაილში ამჯერად ხელით მაქვს გაწერილი მაგრამ უნდა გენერირდებოდეს რენდომ სტრინგი
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            //ტოკენის დაჰეშვა
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //უნდა გავაკეთოთ ტოკენი რომელიც შეიცავს ჩვენს ქლეიმებს და expaired დროს
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            //ტოკენ ჰენდლერი იმისათვის რო გამოვიყენოთ ტოკენის ფუნქციები
            var tokenHandler = new JwtSecurityTokenHandler();

            //ტოკენის შექმნა ზემოთ არსებული პარამეტრებით
            var token = tokenHandler.CreateToken(tokenDescription);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });

        }

    }
}
