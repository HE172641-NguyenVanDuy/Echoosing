using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ICodeJoinClassService
    {
        public string Encode(string guid);
        public string Decode(string base62);
    }

    public class CodeJoinClassService : ICodeJoinClassService
    {
        private readonly string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public string Decode(string base62)
        {
            long value = 0;
            foreach (char c in base62)
            {
                value = value * 62 + Base62Chars.IndexOf(c);
            }

            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes).ToString();
        }

        public string Encode(string guidString)
        {
            Guid guid = new Guid(guidString);
            byte[] bytes = guid.ToByteArray();
            StringBuilder base62 = new StringBuilder();

            // Chuyển đổi mỗi byte thành Base62
            long value = BitConverter.ToInt64(bytes, 0);
            while (value > 0)
            {
                base62.Insert(0, Base62Chars[(int)(value % 62)]);
                value /= 62;
            }

            return base62.ToString();
        }
    }
}
