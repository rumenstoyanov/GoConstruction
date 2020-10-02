using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Constants
{
    /// <summary>
    /// This class containers the property names (precise spellings) to be used in models of resources.
    /// Resources are:
    /// Job
    /// Site
    /// These names allow for re-using of methods taking generic arguments, where reflection methods are used to access properties and property values within the method,
    /// e.g. UpdateService.GetResourceUpdate
    /// </summary>
    public static class FixedPropertyNames
    {
        public const string PrimaryKey = "Id";
        public const string OrganisationId = "Oid";
    }
}
