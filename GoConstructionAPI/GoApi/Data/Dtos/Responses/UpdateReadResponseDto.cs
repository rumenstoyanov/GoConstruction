using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class UpdateReadResponseDto
    {
        public Guid Id { get; set; }
        public Guid UpdatedResourceId { get; set; }
        public DateTime Time { get; set; }
        public List<UpdateDetail> UpdateList { get; set; } = new List<UpdateDetail>();
    }
}
