using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    public class Mail
    {
        public static string ConfirmationSubject(string myCompanyName)
        {
            return $"{myCompanyName} Email Confirmation";
        }
        public static string ConfirmationContractorBody(string username, string orgName, string myCompanyName, string comfirmationLink)
        {
            string outString = @$"Welcome to {myCompanyName}, {orgName}!

Dear {username},
Please confirm your email address by clicking the link: {comfirmationLink}.
Regards,

-- The {myCompanyName} Team";
            return outString;
        }
    }
}
