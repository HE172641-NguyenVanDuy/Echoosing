using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.OTP
{
    public interface ITempOtpStorage
    {
        void SaveOtp(string email, string otp, DateTime expiryTime);
        OtpRecord GetOtpRecord(string email);
        void RemoveOtp(string email);
    }



}
