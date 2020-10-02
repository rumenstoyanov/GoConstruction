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
    public class JobStatusProfile : Profile
    {
        public JobStatusProfile()
        {
            // Source -> Target
            CreateMap<JobStatus, JobStatusReadResponseDto>();
        }
    }
}
