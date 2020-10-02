using GoLibrary.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Dtos
{
    public class SiteCreateRequestDto
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [MaxLength(16)]
        [NoSpaces(ErrorMessage = "Site ID not valid.")]
        public string FriendlyId { get; set; }
    }
}
