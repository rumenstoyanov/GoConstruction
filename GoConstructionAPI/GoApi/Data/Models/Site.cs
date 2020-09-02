using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    public class Site
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Organisation Organisation { get; set; }
        [Required]
        [ForeignKey("Organisation")]
        public Guid Oid { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public string CreatedByUserId { get; set; }
        [Required]
        public ApplicationUser CreatedByUser { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [MaxLength(16)]
        public string FriendlyId { get; set; }
    }
}
