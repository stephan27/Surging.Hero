﻿using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using Surging.Hero.Auth.Domain.Shared;
using Surging.Hero.Auth.IApplication.User;
using Surging.Hero.Auth.IApplication.User.Dtos;
using System.Threading.Tasks;
using Surging.Core.Validation.DataAnnotationValidation;
using Surging.Core.AutoMapper;
using Surging.Hero.Auth.Domain.User;
using Surging.Core.Dapper.Repositories;
using Surging.Core.CPlatform.Exceptions;

namespace Surging.Hero.Auth.Application.User
{
    [ModuleName(ModuleNameConstants.UserModule, Version = ModuleNameConstants.ModuleVersionV1)]
    public class UserAppService : ProxyServiceBase, IUserAppService
    {
        private readonly IUserDomainService _userDomainService;
        private readonly IDapperRepository<UserInfo, long> _userRepository;
        public UserAppService(IUserDomainService userDomainService,
            IDapperRepository<UserInfo, long> userRepository)
        {
            _userDomainService = userDomainService;
            _userRepository = userRepository;
        }


        public async Task<string> CreateUser(CreateUserInput input)
        {
            input.CheckDataAnnotations().CheckValidResult();
            var existUser = await _userRepository.FirstOrDefaultAsync(p => p.UserName == input.UserName);
            if (existUser != null)
            {
                throw new UserFriendlyException($"已经存在用户名为{input.UserName}的用户");
            }
            existUser = await _userRepository.FirstOrDefaultAsync(p => p.Phone == input.Phone);
            if (existUser != null)
            {
                throw new UserFriendlyException($"已经存在手机号码为{input.Phone}的用户");
            }
            existUser = await _userRepository.FirstOrDefaultAsync(p => p.Email == input.Email);
            if (existUser != null)
            {
                throw new UserFriendlyException($"已经存在Email为{input.Email}的用户");
            }
            var userInfo = input.MapTo<UserInfo>();            
            await _userDomainService.CreateUser(userInfo);
            return "新增员工成功";
        }

        public async Task<string> UpdateUser(UpdateUserInput input)
        {
            input.CheckDataAnnotations().CheckValidResult();
            var updateUser = await _userRepository.SingleOrDefaultAsync(p => p.Id == input.Id);
            if (updateUser == null)
            {
                throw new BusinessException($"不存在Id为{input.Id}的账号信息");
            }
            if (input.Phone != updateUser.Phone)
            {
                var existUser = await _userRepository.FirstOrDefaultAsync(p => p.Phone == input.Phone);
                if (existUser != null)
                {
                    throw new UserFriendlyException($"已经存在手机号码为{input.Phone}的用户");
                }
            }
            if (input.Email != updateUser.Email)
            {
                var existUser = await _userRepository.FirstOrDefaultAsync(p => p.Email == input.Email);
                if (existUser != null)
                {
                    throw new UserFriendlyException($"已经存在Email为{input.Email}的用户");
                }
            }
            updateUser = input.MapTo(updateUser);
            await _userRepository.UpdateAsync(updateUser);
            return "更新员工信息成功";
        }
    }
}