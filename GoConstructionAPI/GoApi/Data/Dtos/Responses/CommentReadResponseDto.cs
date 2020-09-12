using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class CommentReadResponseDto
    {
        public AbridgedUserInfoResponseDto PostedByUserInfo { get; set; }
        public DateTime TimePosted { get; set; }
        public string Text { get; set; }
        public List<AbridgedUserInfoResponseDto> UsersTaggedInfo { get; set; } = new List<AbridgedUserInfoResponseDto>();
    }
}
