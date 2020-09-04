using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data.Dtos;
using GoApi.Data.Models;

namespace GoApi.Profiles
{
    public class JobProfile : Profile
    {
        // Source -> Target
        public JobProfile()
        {
            CreateMap<RootJobCreateRequestDto, Job>();
        }
    }
}
