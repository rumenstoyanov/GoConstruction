using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsInitialSet { get; set; }

        [Required]
        [MaxLength(250)]
        public string FullName { get; set; }
    }
}
