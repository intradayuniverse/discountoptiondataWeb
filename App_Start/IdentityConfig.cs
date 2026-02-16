using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using DiscountOptionDataWeb.Models;
using System.Net;
using System.Configuration;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using OptionsDataService;

// using SendGrid's C# Library
// https://github.com/sendgrid/sendgrid-csharp
using SendGrid;


namespace DiscountOptionDataWeb
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            //await configSendGridasync(message);

            //await configSendGoogleEmailasync(message);

            await configSendGridEmailasync(message);

            //await configSendMailGunAsync(message);

            // Plug in your email service here to send an email.
            //return Task.FromResult(0);
        }

        /// <summary>
        /// TEST
        /// lnh: using Google Email service
        /// </summary>
        /// <param name="myMessage"></param>
        /// <returns></returns>
        public async Task configSendGoogleEmailasync(IdentityMessage myMessage)
        {
            var fromAddress = new MailAddress("discountoptiondata@gmail.com", "From discountoptiondata.com");
            var toAddress = new MailAddress(myMessage.Destination, myMessage.Destination);
            string fromPassword = ConfigurationManager.AppSettings["GmailPW"].ToString();
            string subject = myMessage.Subject;
            string body = myMessage.Body;


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

         

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml  = true
            })
            {

                smtp.Send(message);
            }

            //var client = new SmtpClient("smtp.gmail.com",587)
            //{
            //    Credentials = new NetworkCredential("luanhuynh73@gmail.com", "Synot2727!"),
            //    EnableSsl = true
            //};
            //client.Send("luanhuynh73@gmail.com", "luanhuynh73@yahoo.com", "test", "testbody");
            //Console.WriteLine("Sent");
            //Console.ReadLine();


            await Task.FromResult(0);
        }

        public async Task configSendGridEmailasync(IdentityMessage myMessage)
        {
            var apiKey = "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"; // Environment.GetEnvironmentVariable("LuanKey");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("sales@discountoptiondata.com", "Registration with discountoptiondata.com");
            var subject = myMessage.Subject;
            var to = new EmailAddress(myMessage.Destination, myMessage.Subject);
            var plainTextContent = myMessage.Body;
            var htmlContent = myMessage.Body;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);


            try
            {
                var response = await client.SendEmailAsync(msg);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            await Task.FromResult(0);
        }

        //using MailGun
        public async Task configSendMailGunAsync(IdentityMessage myMessage)
        {
            string fromEmail = "sales@discountoptiondata.com";
            List<string> toList = new List<string>();
            toList.Add(myMessage.Destination);
            OptionsDataService.EmailService.SendSimpleMessage(fromEmail, toList, myMessage.Subject, myMessage.Body);

            await Task.FromResult(0);
        }

     
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
