using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class CommentReadResponseDto
    {
        public AbridgedUserInfoResponseDto PostedByUser { get; set; }
        public DateTime TimePosted { get; set; }
        public string Text { get; set; }
        public List<AbridgedUserInfoResponseDto> UsersTagged { get; set; } = new List<AbridgedUserInfoResponseDto>();
    }
}
