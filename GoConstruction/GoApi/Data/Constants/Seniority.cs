using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    public static class Seniority
    {
        public const string SeniorityClaimKey = "Seniority";
        public const string OrganisationIdClaimKey = "oid";
        public const string IsInitalSetClaimKey = "IsInitialSet";

        public const string Admin = "Admin";
        public const string Contractor = "Contractor";
        public const string Manager = "Manager";
        public const string Supervisor = "Supervisor";
        public const string Worker = "Worker";

        public const string AdminOnlyPolicy = "AdminOnly";
        public const string ContractorOrAbovePolicy = "ContractorOrAbove";
        public const string ManagerOrAbovePolicy = "ManagerOrAbove";
        public const string SupervisorOrAbovePolicy = "SupervisorOrAbove";
        public const string WorkerOrAbovePolicy = "WorkerOrAbove";

        public const string RandomPasswordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public const int RandomPasswordLength = 8;
        public const string RandomPasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$";

    }
}
