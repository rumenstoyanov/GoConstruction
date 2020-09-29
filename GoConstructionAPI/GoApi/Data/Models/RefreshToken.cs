using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Token { get; set; }
        [Required]
        public string jti { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        [Required]
        public bool IsInvalidated { get; set; }
        [Required]
        public ApplicationUser User { get; set; }
        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
    }
}
