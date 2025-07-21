using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.OTP
{
    public class InMemoryOtpStorage : ITempOtpStorage
    {
        private readonly Dictionary<string, OtpRecord> _otpStorage = new();

        public void SaveOtp(string email, string otp, DateTime expiryTime)
        {
            _otpStorage[email] = new OtpRecord { OtpCode = otp, ExpiryTime = expiryTime };
        }

        public OtpRecord GetOtpRecord(string email)
        {
            _otpStorage.TryGetValue(email, out var record);
            return record;
        }

        public void RemoveOtp(string email)
        {
            _otpStorage.Remove(email);
        }
    }

    public class OtpRecord
    {
        public string OtpCode { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

}
