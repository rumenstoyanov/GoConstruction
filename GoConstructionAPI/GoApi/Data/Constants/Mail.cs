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
        public static string ConfirmationContractorBody(string fullName, string orgName, string myCompanyName, string comfirmationLink)
        {
            string outString = @$"Welcome to {myCompanyName}, {orgName}!

Dear {fullName},
Please confirm your email address by clicking the link: {comfirmationLink}.
Regards,

-- The {myCompanyName} Team";
            return outString;
        }

        public static string ConfirmationAndPasswordNonContractorBody(string fullName, string orgName, string myCompanyName, string comfirmationLink, string seniority, string password, string inviterFullName)
        {
            string outString = @$"Hello from {myCompanyName}, {fullName}!
{inviterFullName} has invited you to join {orgName}'s organisation on the {myCompanyName} service in the position of {seniority}.
Please confirm your email address by clicking the link: {comfirmationLink}.
You may then log in with this email address at <WEB APP LINK HERE>, your password is: {password}.
Please use this the first time; you will then be prompted to enter some extra information and change your password.
Regards,

-- The {myCompanyName} Team";
            return outString;
        }
    }
}
