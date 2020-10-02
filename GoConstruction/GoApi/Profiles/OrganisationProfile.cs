using AutoMapper;
using GoApi.Data.Dtos;
using GoLibrary.Data.Models;
using GoLibrary.Data.Dtos;
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
            CreateMap<RegisterContractorRequestDto, Organisation>();
            CreateMap<Organisation, OrganisationInfoResponsetDto>();
        }
    }
}
