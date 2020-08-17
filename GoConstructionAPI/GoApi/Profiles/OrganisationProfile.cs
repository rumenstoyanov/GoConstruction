using AutoMapper;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Profiles
{
    public class OrganisationProfile : Profile
    {
        public OrganisationProfile()
        {
            // Source -> Target
            CreateMap<RegisterContractorDto, Organisation>();
        }
    }
}
