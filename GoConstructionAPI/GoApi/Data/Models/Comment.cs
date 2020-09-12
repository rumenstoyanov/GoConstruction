using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Job Job { get; set; }
        [Required]
        [ForeignKey(nameof(Job))]
        public Guid JobId { get; set; }
        [Required]
        public ApplicationUser PostedByUser { get; set; }
        [Required]
        [ForeignKey(nameof(PostedByUser))]
        public string PostedByUserId { get; set; }
        [Required]
        public DateTime TimePosted { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Text { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public List<string> UsersTagged { get; set; } = new List<string>();
    }
}
