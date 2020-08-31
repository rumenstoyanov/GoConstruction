using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class RegisterNonContractorRequestDto
    {
        [Required]
        [ValidEmail(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }

        [Required]
        [MaxLength(250)]
        public string FullName { get; set; }

    }
}
