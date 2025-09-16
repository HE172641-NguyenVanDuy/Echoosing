using BusinessObjects.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Services;
using Services.OTP;
using Services.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        private readonly SendGmail _sendGmailService;
        private readonly GetEmailTemplateServie _getEmailTemplateServie;

		private readonly JwtSettings _jwtSettings;

		public AuthController(TokenService tokenService, 
            IPasswordService passwordService, EchoosingContext context, IAccountService accountService, 
            IConfiguration configuration, ITempOtpStorage otpStorage, SendGmail _sendGmailService, GetEmailTemplateServie _getEmailTemplateServie, IOptions<JwtSettings> jwtSettingsOptions)
        {
            _tokenService = tokenService;
            _passwordService = passwordService;
            _context = context;
            _accountService = accountService;
            _configuration = configuration;
            _otpStorage = otpStorage;
            _sendGmailService = _sendGmailService;
            _getEmailTemplateServie = _getEmailTemplateServie;
			_jwtSettings = jwtSettingsOptions.Value;
		}

		//[HttpPost("login")]
		//public async Task<IActionResult> Login([FromBody] LoginRequest request)
		//{
		//    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
		//    {
		//        return BadRequest(new { message = "Username and password are required." });
		//    }

		//    // Tùy vào cách bạn xử lý hash
		//    var user = await _context.Users
		//        .FirstOrDefaultAsync(u => u.Username == request.Username);

		//    Console.WriteLine(user.ToString());

		//    if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
		//    {
		//        return Unauthorized(new { message = "Invalid username or password." });
		//    }

		//    IConfiguration configuration = new ConfigurationBuilder()
		//            .SetBasePath(Directory.GetCurrentDirectory())
		//            .AddJsonFile("appsettings.json", true, true).Build();

		//    var claims = new List<Claim>
		//        {
		//    new Claim(ClaimTypes.Name, user.Username),
		//    new Claim(ClaimTypes.Email, user.Email),
		//    new Claim("UserID", user.UserId.ToString()),
		//    new Claim("Role", user.Role.ToString() )
		//        };

		//    var symetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]));
		//    var signCredential = new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256);

		//    var preparedToken = new JwtSecurityToken(
		//        issuer: configuration["JwtSettings:Issuer"],
		//        audience: configuration["JwtSettings:Audience"],
		//        claims: claims,
		//        expires: DateTime.Now.AddMinutes(16),
		//        signingCredentials: signCredential);

		//    var generatedToken = new JwtSecurityTokenHandler().WriteToken(preparedToken);
		//    var role = user.Role;
		//    var accountId = user.UserId.ToString();

		//    //var token = _tokenService.GenerateToken(user);

		//    return Ok(new
		//    {
		//        access_token = generatedToken,
		//        token_type = "Bearer",
		//        expires_in = 3600, 
		//        username = user.Username,
		//        role = user.Role
		//    });
		//}
		//    [HttpPost("login")]
		//    public async Task<IActionResult> Login([FromBody] LoginRequest request, IConfiguration configuration)
		//    {
		//        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
		//        {
		//            return BadRequest(new { message = "Username and password are required." });
		//        }

		//        var user = await _context.Users
		//            .FirstOrDefaultAsync(u => u.Username == request.Username);

		//        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
		//        {
		//            return Unauthorized(new { message = "Invalid username or password." });
		//        }

		//        var claims = new List<Claim>
		//{
		//    new Claim(ClaimTypes.Name, user.Username),
		//    new Claim(ClaimTypes.Email, user.Email),
		//    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Sử dụng ClaimTypes.NameIdentifier cho UserID
		//    new Claim(ClaimTypes.Role, user.Role.ToString()) // Sử dụng ClaimTypes.Role cho role
		//};

		//        var symetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]));
		//        var signCredential = new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256);

		//        var token = new JwtSecurityToken(
		//            issuer: configuration["JwtSettings:Issuer"],
		//            audience: configuration["JwtSettings:Audience"],
		//            claims: claims,
		//            expires: DateTime.Now.AddMinutes(60), // Đồng nhất với expires_in
		//            signingCredentials: signCredential);

		//        var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);

		//        return Ok(new
		//        {
		//            access_token = generatedToken,
		//            token_type = "Bearer",
		//            expires_in = 3600, // 1 giờ
		//            username = user.Username,
		//            role = user.Role
		//        });
		//    }

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(new { message = "Username and password are required." });
			}

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

			if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
			{
				return Unauthorized(new { message = "Invalid username or password." });
			}

			// 🔐 Gọi token service để generate JWT
			var token = _tokenService.GenerateToken(user);

			// 🧁 Gán token vào Cookie HttpOnly
			Response.Cookies.Append("JWToken", token, new CookieOptions
			{
				HttpOnly = true,
				Secure = true, // Chỉ bật nếu dùng HTTPS
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
			});

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = 3600, // 1 giờ
                username = user.Username,
                role = user.Role
            });
        }


        [HttpGet("user-list")]

		public async Task<IActionResult> GetUserList()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    UserId = u.UserId,
                    Username = u.Username
                })
                .ToListAsync();

            return Ok(users);
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
		[Authorize]
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
		[Authorize]
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

        [HttpPost("forgot-password-email")]
        public async Task<IActionResult> ForgotPasswordEmail([FromBody] ForgotPasswordEmailRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Tài khoản chưa tồn tại" });
            }

            string errorMsg = _getEmailTemplateServie.GetEmailTemplate("Echoosing.Resources.EmailOTP.html", out string htmlOTP);
            if (errorMsg != null)
            {
                return StatusCode(500, new { message = errorMsg });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            htmlOTP = htmlOTP.Replace("@{OTP}@", otp);

            string sendResult = await _sendGmailService.SendMail(new MailContent
            {
                To = user.Email,
                Sub = "Email xác thực tài khoản.",
                Body = htmlOTP
            });

            if (sendResult != null)
            {
                return StatusCode(500, new { message = sendResult });
            }

            user.Otp = otp;
            user.OtpexpirationTime = DateTime.Now.AddMinutes(5);
            _context.SaveChanges();

            return Ok(new { message = "OTP đã được gửi", userId = user.UserId });
        }



        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] ForgotPasswordOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserID);

            if (user == null)
            {
                return BadRequest("Tài khoản chưa tồn tại.");
            }

            if (user.Otp != request.OTP)
            {
                return BadRequest("Mã OTP không hợp lệ.");
            }

            if (user.OtpexpirationTime < DateTime.Now)
            {
                return BadRequest("Mã OTP đã hết hạn.");
            }

            // OK => Return user ID to redirect on FE
            return Ok(new { userID = user.UserId });
        }
    }
    public class ForgotPasswordOtpRequest
    {
        public string UserID { get; set; }
        public string OTP { get; set; }
    }

    public class ForgotPasswordEmailRequest
    {
        public string Email { get; set; }
    }

}


