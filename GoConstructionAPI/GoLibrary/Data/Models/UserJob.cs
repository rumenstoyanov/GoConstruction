using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Models
{
    public class UserJob
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public ApplicationUser Assignee { get; set; }
        [Required]
        [ForeignKey(nameof(Assignee))]
        public string UserId { get; set; }
        [Required]
        public Job Job { get; set; }
        [Required]
        [ForeignKey(nameof(Job))]
        public Guid JobId { get; set; }
    }
}
