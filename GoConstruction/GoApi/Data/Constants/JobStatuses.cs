using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    public static class JobStatuses
    {
        public static string[] StatusList = new string[] { DefaultStatus, "MATERIALS ON SITE", "LOADED OUT", "IN PROGRESS", "UNDER REVIEW", "DONE" };

        public const string DefaultStatus = "TO DO";


    }
}
