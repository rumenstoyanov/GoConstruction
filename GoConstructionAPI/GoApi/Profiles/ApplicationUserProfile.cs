using AutoMapper;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Profiles
{
    public class ApplicationUserProfile : Profile
    {
        public ApplicationUserProfile()
        {
            // Source -> Target
            CreateMap<ApplicationUser, ApplicationUserInfoResponseDto>();
        }
    }
}
