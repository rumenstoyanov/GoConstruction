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
using GoApi.Data;
using Microsoft.AspNetCore.Http;

namespace GoApi.Services.Implementations
{
    public class UpdateService : IUpdateService
    {
        private readonly IResourceService _resourceService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _appDbContext;
        public UpdateService(IResourceService resourceService, UserManager<ApplicationUser> userManager, AppDbContext appDbContext)
        {
            _resourceService = resourceService;
            _userManager = userManager;
            _appDbContext = appDbContext;

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
            // Assemble the list of all users concerned with this job in stages.
            // May be duplicate users here - these are filtered to be unique after this step but before the emails are sent off.
            var outList = new List<ApplicationUser>();

            async Task AddToOutList(string userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user.IsActive)
                {
                    outList.Add(user);
                }
            }

            // Step 1: Get all the assignees of the job.
            foreach (var uj in _resourceService.GetAssigneeUserIdsForValidJob(job.Id).ToList())
            {
                await AddToOutList(uj.UserId);
            }

            // Step 2: Get all the people who have commented or been tagged in a comment.
            foreach (var comment in _appDbContext.Comments.Where(c => c.JobId == job.Id).ToList())
            {
                await AddToOutList(comment.PostedByUserId);
                foreach (var id in comment.UsersTagged)
                {
                    await AddToOutList(id);
                }
            }

            // Step 3: Get the job creator/owner.
            await AddToOutList(job.Owner.Id);
            return outList; 
        }

        // {Controller}.{Method}.{Id Name}
        // To be parsed when read requests are made - constructing the links fresh.
        private string _userDetailLocation
        {
            get
            {
                return $"Organisation.{nameof(OrganisationController.GetUsersDetail)}.userId";
            }
        }

        public async Task<Update> GetCommentUpdateAsync(ApplicationUser user, Job job, Comment comment)
        {
            var update = new Update
            {
                UpdatedResourceId = job.Id,
                Time = DateTime.UtcNow,
                Oid = job.Oid
            };
            update.UpdateList.Add(new UpdateDetail { Resource = new ResourceUpdateDetail { Id = user.Id, Location = _userDetailLocation, Name = user.FullName }, Syntax = null });
            update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = $" commented:\n\n{comment.Text}\n\n" });
            if (comment.UsersTagged.Any())
            {
                update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = $"Tagged:\n" });
                foreach (var u in comment.UsersTagged)
                {
                    var _user = await _userManager.FindByIdAsync(u);
                    update.UpdateList.Add(new UpdateDetail { Resource = new ResourceUpdateDetail { Id = u, Location = _userDetailLocation, Name = _user.FullName } });
                    update.UpdateList.Add(new UpdateDetail { Resource = null, Syntax = $" ({_user.Email})\n" });
                }
            }
            return update;
        }

        public void RemapLocationLink(UpdateDetail updateDetail, IUrlHelper Url, HttpRequest Request)
        {
            if (updateDetail.Resource != null)
            {
                // First element is the controller
                // Second element is the method
                // Third element is the id name
                string[] paths = updateDetail.Resource.Location.Split('.');
                var location = Url.Action(paths[1], paths[0], new Dictionary<string, string> { { paths[2], updateDetail.Resource.Id } }, Request.Scheme);
                updateDetail.Resource.Location = location;
            }
        }

        public void RemapLocationLink(Update update, IUrlHelper Url, HttpRequest Request)
        {
            foreach (var udet in update.UpdateList)
            {
                RemapLocationLink(udet, Url, Request);
            }
        }
    } 
}
