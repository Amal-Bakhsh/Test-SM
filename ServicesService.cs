using Api.Models;
using AutoMapper;
using DataAccessLayer.Entities;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAccessLayer.Interfaces;

namespace Api.Services
{
    public class ServicesService
    {
        private readonly IMapper _mapper;
        private ICurrentUserService _currentUserService;
        private IRepository<DataAccessLayer.Entities.Services> _repository;
        private readonly SmartmenuDbContext _context;
        private IRepository<DataAccessLayer.Entities.UserServicesMapping> _repositoryMapping;
        private IRepository<Client> _clientRepository;
        private IRepository<Hotel> _hotelRepository;
        private IRepository<User> _userRepository;
        private IRepository<ClientServices> _ClientServicesRepository;
        private IRepository<UserClientMapping> _UserClientMappingRepository;

		public ServicesService(IMapper mapper, ICurrentUserService currentUserService, SmartmenuDbContext context)
        {
            _mapper = mapper;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<ServicesVM> AddAsync(ServicesVM ServicesVM)
        {
            _repository = new ServicesRepository(_context);
           // _clientRepository = new ClientRepository(_context);
            _clientRepository = new ClientRepository(_context);
            ServicesVM ServicesVM1 = null;
            Client client = _clientRepository.GetAsync(ServicesVM.ClientId).Result;
            if (client.MaxService > client.ServiceNum)
            {
                _userRepository = new UserRepository(_context);
                _ClientServicesRepository = new ClientServicesRepository(_context);
                _userRepository = new UserRepository(_context);
                _repositoryMapping = new UserServicesMappingRepository(_context);
                DataAccessLayer.Entities.Services services = _mapper.Map<DataAccessLayer.Entities.Services>(ServicesVM);
                await _repository.AddAsync(services);
                await _repository.SaveChangesAsync();
				client.ServiceNum++;
                await _clientRepository.UpdateAsync(client);
                await _clientRepository.SaveChangesAsync();
                Client currentClient = _currentUserService.Client != null ? _currentUserService.Client : await _clientRepository.GetAsync(ServicesVM.ClientId);
                int serviceId = services.Id;
                DataAccessLayer.Entities.ClientServices clientService = new ClientServices();
                clientService.ServicesId = serviceId;
                clientService.ClientId = currentClient.Id;
                clientService.Status = true;
                await _ClientServicesRepository.AddAsync(clientService);
                await _ClientServicesRepository.SaveChangesAsync();
				//DataAccessLayer.Entities.UserServicesMapping userServiceMapping = new UserServicesMapping();
    //            userServiceMapping.ServicesId = serviceId;
    //            userServiceMapping.UserId = _currentUserService.User.Id;
    //            userServiceMapping.Status = true;
    //            await _repositoryMapping.AddAsync(userServiceMapping);
    //            await _repositoryMapping.SaveChangesAsync();

    //            if (currentClient.MainBranchId == 0)
    //            {
    //                List<Client> clients = await _clientRepository.GetbyFilterAsync(a => a.MainBranchId == currentClient.Id).Result.ToListAsync();
    //                foreach (Client clien in clients)
    //                {
    //                    ClientServices clientServices2 = new ClientServices();
    //                    clientServices2.ServicesId = serviceId;
    //                    clientServices2.ClientId = client.Id;
    //                    clientServices2.Status = true;
    //                    await _ClientServicesRepository.AddAsync(clientServices2);
    //                    await _ClientServicesRepository.SaveChangesAsync();
    //                }
    //            }
                ServicesVM1 = _mapper.Map<ServicesVM>(services);
            }
            return ServicesVM1;
        }

        public async Task<UserServicesMappingVM> AddUserServiceMapping(UserServicesMappingVM userServiceMappingVM)
        {
            _repositoryMapping = new UserServicesMappingRepository(_context);
            _repository = new ServicesRepository(_context);
            //_ClientCategoryRepository = new ClientCategoryRepository(_context);
            UserServicesMapping userMapping = _mapper.Map<UserServicesMapping>(userServiceMappingVM);
            await _repositoryMapping.AddAsync(userMapping);
            await _repositoryMapping.SaveChangesAsync();
            UserServicesMappingVM servicesMappingVM = _mapper.Map<UserServicesMappingVM>(userMapping);
            return servicesMappingVM;


        }

        public async Task<ServicesVM> UpdateAsync(ServicesVM ServicesVM)
        {
            _repository = new ServicesRepository(_context);
            DataAccessLayer.Entities.Services services = _mapper.Map<DataAccessLayer.Entities.Services>(ServicesVM);
            services.LastModifiedBy = Convert.ToString(_currentUserService?.User?.Id);
            services.LastModifiedOn = DateTime.Now;
            await _repository.UpdateAsync(services);
            await _repository.SaveChangesAsync();
            ServicesVM ServicesVM1 = _mapper.Map<ServicesVM>(services);
            return ServicesVM1;
        }

        public async Task<ServicesVM> DeleteAsync(ServicesVM ServicesVM)
        {
            _repository = new ServicesRepository(_context);
            DataAccessLayer.Entities.Services services = _mapper.Map<DataAccessLayer.Entities.Services>(ServicesVM);
            services.LastModifiedBy = Convert.ToString(_currentUserService?.User?.Id);
            services.LastModifiedOn = DateTime.Now;
            await _repository.DeleteAsync(services);
            await _repository.SaveChangesAsync();
            ServicesVM ServicesVM1 = _mapper.Map<ServicesVM>(services);

            return ServicesVM1;
        }

        public async Task<ServicesVM> GetAsync(int id)
        {
            _repository = new ServicesRepository(_context);
            DataAccessLayer.Entities.Services services = await _repository.GetAsync(id);
            ServicesVM ServicesVM1 = _mapper.Map<ServicesVM>(services);

            return ServicesVM1;
        }

        public async Task<List<ClientServicesVM>> GetAsyncByClient(int id)
        {
            //_repository = new ServicesRepository(_context);
            //List<DataAccessLayer.Entities.Services> services = await _repository.GetbyFilterAsync(a => a.ClientId == clientId).Result.ToListAsync();
            ////List<Theme> Theme = await _repository.GetbyFilterAsync(a => a.CategoryId == categoryid).Result.ToListAsync();
            //List<ServicesVM> ServicesVM1 = _mapper.Map<List<ServicesVM>>(services);

            //return ServicesVM1;
            _repository = new ServicesRepository(_context);
            _ClientServicesRepository = new ClientServicesRepository(_context);
            List<ClientServices> cServices = await _ClientServicesRepository
                .GetbyFilterAsync(a => a.ClientId == id
                 && a.Services.IsShown)
                .Result.Include(a => a.Services.Labels)
                .OrderBy(a => a.Services.Priority)
                .ToListAsync();
            List<ClientServicesVM> cServiceVM = _mapper.Map<List<ClientServicesVM>>(cServices);
            return cServiceVM;
        }

        public async Task<List<ServicesVM>> GetAllAsync()
        {
            _repository = new ServicesRepository(_context);
            List<DataAccessLayer.Entities.Services> services = await _repository.GetAllAsync().Result.ToListAsync();
            List<ServicesVM> ServicesVM1 = _mapper.Map<List<ServicesVM>>(services);

            return ServicesVM1;
        }

        public async Task<List<ServicesVM>> GetbyFilterAsync(Expression<Func<DataAccessLayer.Entities.Services, bool>> condition)
        {
            _repository = new ServicesRepository(_context);
            List<DataAccessLayer.Entities.Services> services = await _repository.GetbyFilterAsync(condition).Result.ToListAsync();
            List<ServicesVM> ServicesVM1 = _mapper.Map<List<ServicesVM>>(services);
            return ServicesVM1;
        }

        public async Task<ClientServicesVM> UpdateStatus(ClientServicesVM ServicesVM)
        {
            _repository = new ServicesRepository(_context);
            _ClientServicesRepository = new ClientServicesRepository(_context);
            _repositoryMapping = new UserServicesMappingRepository(_context);
            _userRepository = new UserRepository(_context);

            ClientServices clientServices = _mapper.Map<ClientServices>(ServicesVM);
            await _ClientServicesRepository.UpdateAsync(clientServices);
            await _ClientServicesRepository.SaveChangesAsync();
            
            UserServicesMapping cServices = await _repositoryMapping.GetbyFilterAsync(a => a.ServicesId == ServicesVM.ServicesId).Result.FirstOrDefaultAsync();
            
            if (cServices != null)
            {
                cServices.Status = ServicesVM.Status;
                await _repositoryMapping.UpdateAsync(cServices);
                await _repositoryMapping.SaveChangesAsync();
                User user = await _userRepository.GetbyFilterAsync(a => a.Id == cServices.UserId).Result.FirstOrDefaultAsync();
                user.Status = ServicesVM.Status;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }


            ClientServicesVM ServicesVM1 = _mapper.Map<ClientServicesVM>(clientServices);
            return ServicesVM1;
        }

        public async Task<UserServicesMappingVM> GetServiceByUser(int id)
        {
            //_repository = new ServicesRepository(_context);
            //List<DataAccessLayer.Entities.Services> services = await _repository.GetbyFilterAsync(a => a.ClientId == clientId).Result.ToListAsync();
            ////List<Theme> Theme = await _repository.GetbyFilterAsync(a => a.CategoryId == categoryid).Result.ToListAsync();
            //List<ServicesVM> ServicesVM1 = _mapper.Map<List<ServicesVM>>(services);

            //return ServicesVM1;
            _repositoryMapping = new UserServicesMappingRepository(_context);
            UserServicesMapping cServices = await _repositoryMapping
                .GetbyFilterAsync(a => a.UserId == id)
                .Result.Include(a => a.Services.Labels)
                .OrderBy(a => a.Services.Priority)
                .FirstOrDefaultAsync();
            UserServicesMappingVM cServiceVM = _mapper.Map<UserServicesMappingVM>(cServices);
            return cServiceVM;
        }

    }
}