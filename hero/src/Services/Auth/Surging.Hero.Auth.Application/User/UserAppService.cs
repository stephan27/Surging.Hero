﻿using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using Surging.Hero.Auth.Domain.Shared;
using Surging.Hero.Auth.IApplication.User;
using Surging.Hero.Auth.IApplication.User.Dtos;
using System.Threading.Tasks;
using Surging.Core.Validation.DataAnnotationValidation;
using Surging.Core.AutoMapper;
using Surging.Core.Dapper.Repositories;
using Surging.Core.CPlatform.Exceptions;
using Surging.Hero.Common.Dtos;
using Surging.Core.Domain.PagedAndSorted;
using System.Collections.Generic;
using Surging.Core.Domain.PagedAndSorted.Extensions;
using Surging.Hero.Auth.Domain.Users;
using Surging.Hero.Common;

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


        public async Task<string> Create(CreateUserInput input)
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

        public async Task<string> Update(UpdateUserInput input)
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


        public async Task<string> Delete(DeleteByIdInput input)
        {
            var userInfo = await _userRepository.SingleOrDefaultAsync(p => p.Id == input.Id);
            if (userInfo == null)
            {
                throw new BusinessException($"不存在Id为{input.Id}的账号信息");
            }
            await _userRepository.DeleteAsync(p => p.Id == input.Id);
            return "删除员工成功";
        }

        public async Task<IPagedResult<GetUserOutput>> Query(QueryUserInput query)
        {
            var queryResult = await _userRepository.GetPageAsync(p => p.UserName.Contains(query.SearchKey)
               || p.ChineseName.Contains(query.SearchKey)
               || p.Email.Contains(query.SearchKey)
               || p.Phone.Contains(query.SearchKey),query.PageIndex,query.PageCount);         
            return queryResult.Item1.MapTo<IEnumerable<GetUserOutput>>().GetPagedResult(queryResult.Item2);
        }

        public async Task<string> UpdateStatus(UpdateUserStatusInput input)
        {
            var userInfo = await _userRepository.SingleOrDefaultAsync(p => p.Id == input.Id);
            if (userInfo == null)
            {
                throw new BusinessException($"不存在Id为{input.Id}的账号信息");
            }
            userInfo.Status = input.Status;
            await _userRepository.UpdateAsync(userInfo);
            var tips = "账号激活成功";
            if (input.Status == Status.Invalid)
            {
                tips = "账号冻结成功";
            }
            return tips;
        }

        public async Task<string> ResetPassword(ResetPasswordInput input)
        {
            var userInfo = await _userRepository.SingleOrDefaultAsync(p => p.Id == input.Id);
            if (userInfo == null)
            {
                throw new BusinessException($"不存在Id为{input.Id}的账号信息");
            }
            await _userDomainService.ResetPassword(userInfo, input.NewPassword);
            return "重置该员工密码成功";
        }

        public async Task<IEnumerable<GetUserOutput>> GetDepartmentUser(long deptId)
        {
            var departUsers = await _userRepository.GetAllAsync(p => p.DeptId == deptId);
            return departUsers.MapTo<IEnumerable<GetUserOutput>>();
        }

        public async Task<IEnumerable<GetUserOutput>> GetCorporationUser(long corporationId)
        {
            var corporationUsers = await _userRepository.GetAllAsync(p => p.DeptId == corporationId);
            return corporationUsers.MapTo<IEnumerable<GetUserOutput>>();
        }
    }
}
