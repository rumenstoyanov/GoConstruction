using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    public class Update
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public List<UpdateDetail> UpdateList { get; set; }
    }

    public class UpdateDetail
    {
        public ResourceUpdateDetail Resource { get; set; }
        public string Syntax { get; set; }
    }

    public class ResourceUpdateDetail
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Id { get; set; }
    }
}
