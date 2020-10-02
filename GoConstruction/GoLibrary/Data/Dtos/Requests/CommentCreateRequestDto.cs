using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoLibrary.Data.Dtos
{
    public class CommentCreateRequestDto
    {
        [Required]
        [MaxLength(4000)]
        public string Text { get; set; }
        [Required]
        public List<string> UsersTagged { get; set; } = new List<string>();
    }
}
