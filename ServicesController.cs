using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using AutoMapper;
using DataAccessLayer.Context;
using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

    public class ServicesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ICurrentUserService _currentUserService;
        private readonly SmartmenuDbContext _context;
        private ServicesService _servicesService;
        private string _contentRootPath;
        public ServicesController(
            IMapper mapper,
            ICurrentUserService currentUserService,
            SmartmenuDbContext context,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _context = context;
            _currentUserService = currentUserService;
            _contentRootPath = env.ContentRootPath;
        }


        [Route("services/add")]
        [Authorize(Roles = "Admin,Restaurant,Internal")]
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] ServicesVM model)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.AddAsync(model);
            return Ok(data);
        }

        [Route("services/addusermapping")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost]
        public async Task<IActionResult> AddUSerMappigAsync([FromBody] UserServicesMappingVM model)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.AddUserServiceMapping(model);
            return Ok(data);
        }
        [Route("services/update")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost]
        public async Task<IActionResult> UpdateAsync([FromBody] ServicesVM model)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.UpdateAsync(model);
            return Ok(data);
        }


        [Route("services/get/{id}")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpGet]
        public async Task<IActionResult> GetAsync(int id)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.GetAsync(id);
            return Ok(data);
        }

        [Route("services/getlist")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.GetAllAsync();
            return Ok(data);
        }

        [Route("services/getbyclient/{id}")]
        [Authorize(Roles = "Admin,Restaurant,Customer")]
        [HttpGet]
        public async Task<IActionResult> getByClient(int id)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.GetAsyncByClient(id);
            return Ok(data);
        }

  
        [Route("services/updatepriority")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost]
        public async Task<IActionResult> UpdatePriorityAsync([FromBody] List<ServicesVM> models)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);
            List<ServicesVM> data = new List<ServicesVM>();
            foreach (ServicesVM model in models)
            {
                data.Add(await _servicesService.UpdateAsync(model));
            }
            return Ok(data);
        }


        [Route("services/updatestatus")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] ClientServicesVM model)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.UpdateStatus(model);
            return Ok(data);
        }
        [Route("services/upload")]
        [Authorize(Roles = "Admin,Restaurant")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Services", "Images");
                var pathToSave = Path.Combine(_contentRootPath + "\\wwwroot\\images", "services");
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Route("services/getservicebyuser")]
        [Authorize(Roles = "Admin,Restaurant,Service")]
        [HttpPost]
        public async Task<IActionResult> GetServiceByUser([FromBody] int userId)
        {
            _servicesService = new ServicesService(_mapper, _currentUserService, _context);

            var data = await _servicesService.GetServiceByUser(userId);
            return Ok(data);
        }
    }
}