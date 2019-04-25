using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OAuthTutorial.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class LocalFileEmailSender : IEmailSender
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public LocalFileEmailSender(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Emails", email);
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            await File.WriteAllTextAsync(Path.Combine(folderPath, DateTime.UtcNow.Ticks + "_" + subject + ".html"), message);
        }
    }
}
