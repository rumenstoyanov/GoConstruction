using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Dtos
{
    public class SiteUpdateRequestDto
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
}
