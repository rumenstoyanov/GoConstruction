using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace GoLibrary.Data.Models
{
    public class JobStatus
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        public List<Job> Jobs { get; set; }
    }
}
