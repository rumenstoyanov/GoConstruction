using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Data.Dtos
{
    public class AuthInternalDto
    {
        public bool Success { get; set; }
        public List<IdentityError> Errors { get; set; } = null;
    }
}
