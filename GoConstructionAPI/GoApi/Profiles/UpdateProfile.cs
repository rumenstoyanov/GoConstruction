﻿using AutoMapper;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
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
