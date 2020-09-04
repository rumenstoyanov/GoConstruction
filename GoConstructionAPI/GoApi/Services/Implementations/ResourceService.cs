using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace GoApi.Services.Implementations
{
    public class ResourceService : IResourceService
    {
        private readonly AppDbContext _appDbContext;

        public ResourceService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public string GenerateJobFriendlyId(Site site)
        {
            int maxId = site.Jobs.Count;
            int newId = maxId + 1;
            return $"{site.FriendlyId}-{newId}";
        }

        public int GetDefaultJobStatusId()
        {
            var defaultStatus = _appDbContext.JobStatuses.SingleOrDefault(js => js.Title == JobStatuses.DefaultStatus);
            return defaultStatus.Id;

        }
    }
}
