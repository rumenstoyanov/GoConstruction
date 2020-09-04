using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface IResourceService
    {
        string GenerateJobFriendlyId(Site site);

        public int GetDefaultJobStatusId();
    }
}
