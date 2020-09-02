using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    public class Mail
    {
        public static string ConfirmationSubject(string myCompanyName)
        {
            return $"{myCompanyName} Email Confirmation";
        }

        public static string ResetPasswordSubject(string myCompanyName)
        {
            return $"{myCompanyName} Password Reset";
        }

        public static string SiteUpdateSubject(string myCompanyName, string siteFriendlyId)
        {
            return $"Site {siteFriendlyId} Update ({myCompanyName})";
        }

        private static string Salutation(string myCompanyName)
        {
            string outString = @$"
Regards,

-- The {myCompanyName} Team";
            return outString;
        }
        public static string ConfirmationContractorBody(string fullName, string orgName, string myCompanyName, string comfirmationLink)
        {
            string outString = @$"Welcome to {myCompanyName}, {orgName}!

Dear {fullName},
Please confirm your email address by clicking the link: {comfirmationLink}.
{Salutation(myCompanyName)}";
            return outString;
        }

        public static string ConfirmationAndPasswordNonContractorBody(string fullName, string orgName, string myCompanyName, string comfirmationLink, string seniority, string password, string inviterFullName)
        {
            string outString = @$"Hello from {myCompanyName}, {fullName}!
{inviterFullName} has invited you to join {orgName}'s organisation on the {myCompanyName} service in the position of {seniority}.
Please confirm your email address by clicking the link: {comfirmationLink}.
You may then log in with this email address at <WEB APP LINK HERE>, your password is: {password}.
Please use this the first time; you will then be prompted to enter some extra information and change your password.
{Salutation(myCompanyName)}";
            return outString;
        }


        public static string ResetPasswordBody(string fullName, string myCompanyName, string newPassword)
        {
            string outString = @$"Dear {fullName},
You made a request to reset to reset your {myCompanyName} password. You new password is {newPassword}.
Please use this to log in at <WEB APP LINK HERE>.
If you did not make this request please contact support <EMAIL HERE>.
{Salutation(myCompanyName)}";
            return outString;
        }

        public static string SiteUpdate(string update, string siteTitle, string siteFriendlyId)
        {
            string outString = @$"Site {siteFriendlyId} - {siteTitle}

{update}
";
            return outString;
        }


    }
}
