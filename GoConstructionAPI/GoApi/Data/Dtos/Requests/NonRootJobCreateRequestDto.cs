using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class NonRootJobCreateRequestDto
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        [Required]
        public Guid ParentJobId { get; set; }
    }
}
