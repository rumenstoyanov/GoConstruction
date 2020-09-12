using AutoMapper;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            // Source -> Target
            CreateMap<CommentCreateRequestDto, Comment>();
        }
    }
}
