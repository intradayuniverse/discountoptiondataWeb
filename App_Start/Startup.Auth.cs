using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using DiscountOptionDataWeb.Models;
using System.Configuration;

namespace DiscountOptionDataWeb
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");



            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //****move to web.config
            //facebook developer only allow 1 site? can we do more than1, one for dev, one for prod?
            //about 
            //1) for  appname:discountoptiondatahc1 , siteUrl: https://www.discountoptiondata.com/
            //damn, would not work this way, facebook dev interface only allow 1 site at a time
            //google allow 2 sites
            //this way there'll be 2 buttons
            //app.UseFacebookAuthentication(
            //  appId: "1874360382846825",
            //  appSecret: "9d65a09788dc588d85fab9d63f384d4e");

            ////2) for  appname:discountoptiondatalive2 , siteUrl: https://discountoptiondata.com/
            //app.UseFacebookAuthentication(
            //  appId: "637838333087442",
            //  appSecret: "b576e598c78424febbfa5d17fe65359d");

            app.UseFacebookAuthentication(
             appId: ConfigurationManager.AppSettings["FacebookKey"].ToString(),
             appSecret: ConfigurationManager.AppSettings["FacebookSecret"].ToString());


            ///for luanhuynh2727@gmail.com account, need to set up for live account
            ///at discountoptiondata@gmail.com
            ///https://console.developers.google.com/apis/credentials/oauthclient/420949011091-0h925d2bu660uoa5d2oajoivo7npssu1.apps.googleusercontent.com?project=420949011091
            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "420949011091-0h925d2bu660uoa5d2oajoivo7npssu1.apps.googleusercontent.com",
            //    ClientSecret = "-FNUoLIbrP2AdwfRl0IvM_y8"
            //});

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GoogleKey"].ToString(),
                ClientSecret = ConfigurationManager.AppSettings["GoogleSecret"].ToString()
            });
        }
    }
}