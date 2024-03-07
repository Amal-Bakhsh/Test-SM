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
using System.Text.Json;

namespace Api.Services
{
    public class PermissionsService
    {
        private readonly IMapper _mapper;
        private ICurrentUserService _currentUserService;
        private IRepository<PermissionsGroup> _repository;
        private IRepository<PermissionsCategory> _PermissionsCategoryRepository;
        private IRepository<Permissions> _PermissionsRepository;
        private IRepository<User> _UserRepository;
        private readonly SmartmenuDbContext _context;

        public PermissionsService(IMapper mapper, ICurrentUserService currentUserService, SmartmenuDbContext context)
        {
            _mapper = mapper;
            _currentUserService = currentUserService;
            _context = context;
        }


        public async Task<List<PermissionsCategoryVM>> GetAllCategoriesAsync()
        {
            _PermissionsCategoryRepository = new PermissionsCategoryRepository(_context);
            List<PermissionsCategory> Category = await _PermissionsCategoryRepository.GetAllAsync().Result.ToListAsync();
            List<PermissionsCategoryVM> CategoryVM1 = _mapper.Map<List<PermissionsCategoryVM>>(Category);
            return CategoryVM1;
        }

        public async Task<List<PermissionsVM>> GetAllPermissionsAsync()
        {
            _PermissionsRepository = new PermissionsRepository(_context);
            List<Permissions> Permissions = await _PermissionsRepository.GetAllAsync().Result.ToListAsync();
            List<PermissionsVM> PermissionsVM1 = _mapper.Map<List<PermissionsVM>>(Permissions);
            return PermissionsVM1;
        }

        public async Task<UserVM> GetGroupById(int id)
        {
            _repository = new PermissionsGroupRepository(_context);
            _UserRepository = new UserRepository(_context);
            User user = _UserRepository.GetAsync(id).Result;
            PermissionsGroup Group = await _repository.GetbyFilterAsync(a => a.Id == user.PermissionGroupId).Result.FirstOrDefaultAsync();
            PermissionsGroupVM GroupVM1 = _mapper.Map<PermissionsGroupVM>(Group);
            UserVM userVM = _mapper.Map<UserVM>(user);
            userVM.PermissionsGroup = GroupVM1;
            return userVM;
        }

        public async Task<UserVM> UpdatPG(UserVM userVM)
        {
            _repository = new PermissionsGroupRepository(_context);
            _UserRepository = new UserRepository(_context);
            User user = _UserRepository.GetAsync(userVM.Id).Result;
            PermissionsGroup Group = _mapper.Map<PermissionsGroup>(userVM.PermissionsGroup);
            if(Group.Name == "Admin" || Group.Name == "Default" 
                || Group.Name == "Operation" || Group.Name == "Waiter"
                || Group.Name == "Supervisor" || Group.Name == "Maintenance"
                 || Group.Name == "Inspector" || Group.Name == "Service")
            {
                Group.Id = 0;
                Group.Name = userVM.UserName + "PG";
                await _repository.AddAsync(Group);
                await _repository.SaveChangesAsync();
                user.PermissionGroupId = Group.Id;
                await _UserRepository.UpdateAsync(user);
                await _UserRepository.SaveChangesAsync();
            }
            else
            {
                await _repository.UpdateAsync(Group);
                await _repository.SaveChangesAsync();
            }
            PermissionsGroupVM GroupVM1 = _mapper.Map<PermissionsGroupVM>(Group);
            UserVM userVM1 = _mapper.Map<UserVM>(user);
            userVM1.PermissionsGroup = GroupVM1;
            return userVM1;
        }

        public async Task<List<PermissionsGroupVM>> GetinternalUsersPG()
        {
            _repository = new PermissionsGroupRepository(_context);
            _UserRepository = new UserRepository(_context);
            List<PermissionsGroup> Group = await _repository.GetbyFilterAsync(a => a.Name == "Operation" || a.Name == "Waiter" || a.Name == "Supervisor" || a.Name== "Inspector" || a.Name== "Maintenance" || a.Name == "Service").Result.ToListAsync();
            List<PermissionsGroupVM> GroupVM1 = _mapper.Map<List<PermissionsGroupVM>>(Group);
            return GroupVM1;
        }
    }
}
