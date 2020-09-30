using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data.Dtos;
using GoLibrary.Data.Models;
using GoLibrary.Data.Dtos;

namespace GoApi.Profiles
{
    public class JobProfile : Profile
    {
        
        public JobProfile()
        {
            // Source -> Target
            CreateMap<RootJobCreateRequestDto, Job>();
            CreateMap<Job, JobReadResponseDto>();
            CreateMap<NonRootJobCreateRequestDto, Job>();
            CreateMap<JobUpdateRequestDto, Job>();
            CreateMap<Job, JobUpdateRequestDto>();
            CreateMap<JobUpdateRequestDto, JobUpdateInternalDto>();
        }
    }
}
