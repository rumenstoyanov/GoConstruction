using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoApi.Data.Models
{
    // All readable (the "Name" and "Syntax" fields) except the first start with a space .
    public class Update
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public List<UpdateDetail> UpdateList { get; set; } = new List<UpdateDetail>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Time.ToString("g", new CultureInfo("es-ES"))}: "); // We add the space at the end here since all UpdateDetail entries except the first in the UpdateList start with a space.
            foreach (var detail in UpdateList)
            {
                sb.Append(detail.ToString());
            }
            return sb.ToString();
        }
    }

    public class UpdateDetail
    {
        public ResourceUpdateDetail Resource { get; set; }
        public string Syntax { get; set; }

        public override string ToString()
        {
            if (Resource != null)
            {
                return Resource.Name;
            }
            else
            {
                return Syntax;
            }
            
        }
    }

    public class ResourceUpdateDetail
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Id { get; set; }
    }
}
