using AutoMapper;
using GoApi.Data.Dtos;
using GoLibrary.Data.Models;
using GoLibrary.Data.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Profiles
{
    public class UpdateProfile : Profile
    {
        public UpdateProfile()
        {
            // Source -> Target
            CreateMap<Update, UpdateReadResponseDto>();
        }
    }
}
