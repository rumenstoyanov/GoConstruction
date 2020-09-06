using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class JobReadResponseDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FriendlyId { get; set; }
        public int JobStatusId { get; set; }
        public DateTime DueDate { get; set; }
        public Guid? ParentJobId { get; set; }
    }
}
