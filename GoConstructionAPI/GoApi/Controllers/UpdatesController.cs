using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GoApi.Data;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using GoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UpdatesController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IUpdateService _updateService;

        public UpdatesController(
            IAuthService authService,
            AppDbContext appDbContext,
            IMapper mapper,
            IUpdateService updateService
            )
        {
            _authService = authService;
            _appDbContext = appDbContext;
            _mapper = mapper;
            _updateService = updateService;
        }

        [HttpGet("{resourceId}")]
        [Authorize(Policy = Seniority.WorkerOrAbovePolicy)]
        public IActionResult GetUpdates(Guid resourceId)
        {
            var oid = _authService.GetRequestOid(Request);
            var updates = _appDbContext.Updates.Where(u => u.Oid == oid && u.UpdatedResourceId == resourceId); // Note that updates for inactive resources will still be retrievable,
                                                                                                               // as we do not store active status since this is a general update table.
            foreach (var u in updates) _updateService.RemapLocationLink(u, Url, Request);
            var mappedUpdates = _mapper.Map<IEnumerable<UpdateReadResponseDto>>(updates);
            return Ok(mappedUpdates.OrderBy(u => u.Time));
        }
    }
}
