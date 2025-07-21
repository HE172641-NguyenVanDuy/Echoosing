using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IGetEmailTemplateService
    {
        public string GetEmailTemplate(string resourceName, out string html);
    }
    public class GetEmailTemplateServie : IGetEmailTemplateService
    {
        public string GetEmailTemplate(string resourceName, out string html)
        {
            html = null;
            string msg = null;
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames()
                                       .FirstOrDefault(name => name.EndsWith(resourceName));

            if (resourcePath == null)
            {
                msg = "Không tìm thấy file resource: " + resourceName;
                return msg;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                html = reader.ReadToEnd();
            }
            return msg;
        }
    }
}
