using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Models
{
    public class Job
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Organisation Organisation { get; set; }
        [Required]
        [ForeignKey(nameof(Organisation))]
        public Guid Oid { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public Site Site { get; set; }
        [Required]
        [ForeignKey(nameof(Site))]
        public Guid SiteId { get; set; }
        [Required]
        public ApplicationUser Owner { get; set; }
        [Required]
        [ForeignKey(nameof(Owner))]
        public string OwnerId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        [Required]
        public string FriendlyId { get; set; }
        [Required]
        public JobStatus JobStatus { get; set; }
        [Required]
        [ForeignKey(nameof(JobStatus))]
        public int JobStatusId { get; set; }
        public DateTime DueDate { get; set; }
        public Job ParentJob { get; set; }
        [ForeignKey(nameof(ParentJob))]
        public Guid? ParentJobId { get; set; }

        public List<Job> Jobs { get; set; }
        public List<UserJob> Assignments { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
