using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using Todo.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]//不用驗證
    public class LoginController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        private readonly IConfiguration _configuration;

        public LoginController(TodoContext todoContext,
            IConfiguration configuration)
        {
            _todoContext = todoContext;
            _configuration = configuration;
        }


        [HttpPost]
        public string login(LoginPost value)
        {
            var user = (from a in _todoContext.Employees
                        where a.Account == value.Account
                        && a.Password == value.Password
                        select a).SingleOrDefault();

            if (user == null)
            {
                return "帳號密碼錯誤";
            }
            else
            {
                //這邊等等寫驗證
                var claims = new List<Claim>
                {                 

                    new Claim(ClaimTypes.Name, user.Account),
                    new Claim("FullName", user.Name),
                    new Claim("EmployeeId", user.EmployeeId.ToString())
                };

                var role = from a in _todoContext.Roles
                           where a.EmployeeId == user.EmployeeId
                           select a;

                foreach (var temp in role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, temp.Name));
                }
                //時間設置
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(2)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                //時間設置(加在這邊是指從這裡登入後 她的期限只有兩秒 如果你有其他登入的api救不回售這邊影響)
                //把時間設置寫在這裡跟program.cs不同,program.cs是全部
                //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return "OK";
            }
        }

        [HttpPost("jwtLogin")]
        public string jwtLogin(LoginPost value)
        {
            var user = (from a in _todoContext.Employees
                        where a.Account == value.Account
                        && a.Password == value.Password
                        select a).SingleOrDefault();

            if (user == null)
            {
                return "帳號密碼錯誤";
            }
            else
            {

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Account),
                    new Claim("FullName", user.Name),
                    new Claim(JwtRegisteredClaimNames.NameId, user.EmployeeId.ToString()),
                    new Claim("EmployeeId", user.EmployeeId.ToString())
                };

                var role = from a in _todoContext.Roles
                           where a.EmployeeId == user.EmployeeId
                           select a;

                foreach (var temp in role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, temp.Name));
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

                var jwt = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);


                return token;
            }
        }

        [HttpDelete]
        public void logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        [HttpGet("NoLogin")]
        public string noLogin()
        {
            return "未登入";
        }

        [HttpGet("NoAccess")]
        public string noAccess()
        {
            return "沒有權限";
        }

    }

}
