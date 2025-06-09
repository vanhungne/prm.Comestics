using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Model;
using Service.Interface;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class authenController : ControllerBase
    {
        private readonly IAuthenService _authenServices;

        public authenController(IAuthenService authenServices)
        {
            _authenServices = authenServices;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { code = 400, message = "Invalid username or password" });
            }

            try
            {

                var token = await _authenServices.Login(model);

                if (string.IsNullOrEmpty((string?)token))
                {
                    return Unauthorized(new { code = 401, message = "Invalid username or password" });
                }

                return Ok(new { code = 200, message = "Login successful",token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { code = 500, message = ex.Message });
            }
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel registerDTO)
        {
            if (registerDTO == null || string.IsNullOrEmpty(registerDTO.Email) || string.IsNullOrEmpty(registerDTO.Password))
            {
                return BadRequest(new { code = 400, message = "Invalid registration data" });
            }
            try
            {
                var result = await _authenServices.Register(registerDTO);
                if (string.IsNullOrEmpty((string?)result))
                {
                    return BadRequest(new { code = 400, message = "Registration failed" });
                }
                return Ok(new { code = 200, message = "Registration successful", result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { code = 500, message = ex.Message });
            }
        }

    }
}
