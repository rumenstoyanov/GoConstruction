using GoApi.Data.Dtos;
using GoApi.Data.Models;
using GoApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoApi.Data.Constants;
using Microsoft.AspNetCore.Mvc;
using GoApi.Controllers;
using Microsoft.AspNetCore.Identity;

namespace GoApi.Services.Implementations
{
    public class UpdateService : IUpdateService
    {
        private readonly IResourceService _resourceService;
        private readonly UserManager<ApplicationUser> _userManager;
        public UpdateService(IResourceService resourceService, UserManager<ApplicationUser> userManager)
        {
            _resourceService = resourceService;
            _userManager = userManager;

        }
        public string AssembleSyntaxFromDiff(Dictionary<string, string> diff)
        {

            var sb = new StringBuilder();
            foreach (var key in diff.Keys.SkipLast(1))
            {
                sb.Append($" updated the {key} to {diff[key]},");
            }

            var lastKey = diff.Keys.Last();
            sb.Append($" updated the {lastKey} to {diff[lastKey]}");
            return sb.ToString();

        }

        public Dictionary<string, string> Diff<T>(T preUpdate, T postUpdate) where T : class
        {
            var diffDict = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach (var pi in properties)
            {
                if (pi.GetValue(preUpdate).ToString() != pi.GetValue(postUpdate).ToString())
                {
                    diffDict.Add(pi.Name, pi.GetValue(postUpdate).ToString());
                }
            }
            return diffDict;

        }


        public Update GetResourceUpdate<T, U>(ApplicationUser user, T resource, U preUpdate, U postUpdate, string userDetailLocation)
            where T : class
            where U : class
        {
            var diff = Diff(preUpdate, postUpdate);
            if (diff.Any())
            {
                var syntax = AssembleSyntaxFromDiff(diff);
                var update = new Update
                {
                    UpdatedResourceId = Guid.Parse(resource.GetType().GetProperty(FixedPropertyNames.PrimaryKey).GetValue(resource).ToString()),
                    Time = DateTime.UtcNow,
                    Oid = Guid.Parse(resource.GetType().GetProperty(FixedPropertyNames.OrganisationId).GetValue(resource).ToString())
                };
                update.UpdateList.Add(new UpdateDetail { Resource = new ResourceUpdateDetail { Id = user.Id, Location = userDetailLocation, Name = user.FullName }, Syntax = null });
                update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = syntax });
                return update;
            }
            return null;
        }

        public Update GetAssigneeUpdate(ApplicationUser user, Job job, ApplicationUser updatedUser, string userDetailLocation, string updatedUserDetailLocation, bool isAddition)
        {
            var update = new Update
            {
                UpdatedResourceId = job.Id,
                Time = DateTime.UtcNow,
                Oid = job.Oid
            };
            update.UpdateList.Add(new UpdateDetail { Resource = new ResourceUpdateDetail { Id = user.Id, Location = userDetailLocation, Name = user.FullName }, Syntax = null });
            update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = isAddition ? " assigned the job to " : " removed " });
            update.UpdateList.Add(new UpdateDetail { Resource = new ResourceUpdateDetail { Id = updatedUser.Id, Location = updatedUserDetailLocation, Name = updatedUser.FullName }, Syntax = null });
            update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = isAddition ? "." : " from the assignees for this job."  });
            return update;
        }

        public List<ApplicationUser> GetSiteUpdateRecipients(Site site)
        {
            return new List<ApplicationUser> { site.CreatedByUser };
            
        }

        public async Task<IEnumerable<ApplicationUser>> GetJobUpdateRecipientsAsync(Job job)
        {
            var outList = new List<ApplicationUser>();
            foreach (var uj in _resourceService.GetAssigneeUserIdsForValidJob(job.Id).ToList())
            {
                var user = await _userManager.FindByIdAsync(uj.UserId);
                if (user.IsActive)
                {
                    outList.Add(user);
                }
            }
            outList.Add(job.Owner);
            return outList; // May be duplicate users here - these are filtered to be unique after this step but before the emails are sent off.
        }



    } 
}
