using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.ViewModels;
using System.Security.Claims;

namespace EChoosing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IClassService _classService;

        public HomeController(IClassService classService)
        {
            _classService = classService;
        }


        [HttpGet("role")]
        [Authorize]
        public ActionResult<string> GetUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
            {
                return NotFound("Role not found");
            }
            return Ok(role);
        }

        [HttpPost("join")]
        [Authorize]
        public IActionResult JoinClass([FromBody] JoinClassRequest request)
        {
            var userID = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("UserID not found in token.");
            }

            string msg = _classService.JoinClass(request.ClassCode, userID, out Class classInfo);

            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { message = msg });
            }

            return Ok(new
            {
                message = "Joined successfully",
                classId = classInfo.ClassId
            });
        }


    }
    public class JoinClassRequest
    {
        public string ClassCode { get; set; }
    }

}
