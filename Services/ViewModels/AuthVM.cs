using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ViewModels
{
    public class AuthVM
    {
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {

    }


	public class RegisterUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
    }



    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class OtpRequestDto
    {
        public string Email { get; set; }
    }

    public class OtpVerifyRequest
    {
        public string Email { get; set; } // để tìm OTP theo email
        public string OTP { get; set; }
    }

}
