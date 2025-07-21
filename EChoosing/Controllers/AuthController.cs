using BusinessObjects.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Services;
using Services.OTP;
using Services.ViewModels;

namespace EChoosing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        private readonly IPasswordService _passwordService;

        private readonly EchoosingContext _context;

        private readonly IAccountService _accountService;

        private readonly IConfiguration _configuration;

        private readonly ITempOtpStorage _otpStorage;

        public AuthController(TokenService tokenService, IPasswordService passwordService, EchoosingContext context, IAccountService accountService, IConfiguration configuration, ITempOtpStorage otpStorage)
        {
            _tokenService = tokenService;
            _passwordService = passwordService;
            _context = context;
            _accountService = accountService;
            _configuration = configuration;
            _otpStorage = otpStorage;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            // Tùy vào cách bạn xử lý hash
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = 3600, 
                username = user.Username,
                role = user.Role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra email đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict(new { message = "Email already exists." });
            }

            // Tạo người dùng mới
            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Role = request.SelectedRole,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserID);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.PasswordHash = _passwordService.HashPassword(request.Password);
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successful. Please login again." });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Không thực sự sign out vì JWT không có session
            // FE sẽ xóa token trong localStorage/cookie
            return Ok(new { message = "Logout successful. Please remove token on client." });
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            string userId = _accountService.GetUserIDLogin(HttpContext); // Lấy từ JWT claims
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Login required." });

            string msg = _accountService.GetUserByUserID(userId, out User user);
            if (!string.IsNullOrEmpty(msg) || user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        // PUT: api/profile/change-password
        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { message = "New password must not be empty." });

            string userId = _accountService.GetUserIDLogin(HttpContext);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Login required." });

            string msg = _accountService.GetUserByUserID(userId, out User user);
            if (!string.IsNullOrEmpty(msg) || user == null)
                return NotFound(new { message = "User not found." });

            user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
            user.UpdatedDate = DateTime.UtcNow;

            msg = _accountService.UpdateUserLogin(user);
            if (!string.IsNullOrEmpty(msg))
                return StatusCode(500, new { message = "Failed to update password." });

            return Ok(new { message = "Password updated successfully." });
        }
        [HttpPost("request-otp")]
        public IActionResult RequestOtp([FromBody] OtpRequestDto model)
        {
            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
                return BadRequest(new { message = "Invalid email format." });

            if (_context.Users.Any(u => u.Email == model.Email))
                return Conflict(new { message = "Email already registered." });

            // Generate OTP
            var rnd = new Random();
            int otp = rnd.Next(100000, 999999);
            var expiry = DateTime.UtcNow.AddMinutes(2);

            try
            {
                SendEmail(model.Email, otp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
            }

            // Trả về OTP & expiry nếu bạn muốn FE xử lý
            // Hoặc bạn lưu OTP vào DB nếu cần xác minh tiếp
            return Ok(new
            {
                otp = otp.ToString(),
                email = model.Email,
                expiresAt = expiry
            });
        }

        private void SendEmail(string email, int otp)
        {
            var smtpSettings = _configuration.GetSection("MailSetting");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings["Name"], smtpSettings["Mail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "🔐 Your OTP Code - MultipleChoiceTest";

            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #2c3e50;'>🔐 Your OTP Verification Code</h2>
                    <p>Hello,</p>
                    <p>Thank you for signing up with <b>MultipleChoiceTest</b>. Please use the following OTP code to verify your account:</p>
                    <p style='font-size: 24px; font-weight: bold; color: red; text-decoration: underline; text-align: center;'>
                        {otp}
                    </p>
                    <p>This code will expire in <b>2 minutes</b>. Please do not share this code with anyone.</p>
                    <p>If you did not request this, please ignore this email.</p>
                    <hr style='border: 1px solid #ddd;'/>
                    <p style='font-size: 14px; color: #555;'>Best regards,<br><b>MultipleChoiceTest Team</b></p>
                </div>";

            message.Body = new TextPart("html") { Text = emailBody };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpSettings["Host"], int.Parse(smtpSettings["Port"]), SecureSocketOptions.StartTls);
                client.Authenticate(smtpSettings["Mail"], smtpSettings["Password"]);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }


}


