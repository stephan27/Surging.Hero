﻿using System;
using System.Data.Common;
using System.Threading.Tasks;
using Surging.Core.AutoMapper;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.Dapper.Manager;
using Surging.Core.Dapper.Repositories;
using Surging.Hero.BasicData.Domain.Shared.Wordbooks;
using Surging.Hero.BasicData.IApplication.Wordbook;
using Surging.Hero.BasicData.IApplication.Wordbook.Dtos;
using Surging.Hero.Organization.IApplication.Position.Dtos;

namespace Surging.Hero.Organization.Domain.Positions
{
    public class PositionDomainService : ManagerBase, IPositionDomainService
    {
        private readonly IDapperRepository<Position, long> _positionRepository;

        public PositionDomainService(IDapperRepository<Position, long> positionRepository) {
            _positionRepository = positionRepository;
        }

        public async Task CreatePosition(CreatePositionInput input, DbConnection conn, DbTransaction trans)
        {
            await CheckPosition(input);
            var position = input.MapTo<Position>();
            await _positionRepository.InsertAsync(position, conn, trans);
        }

        public async Task CreatePosition(CreatePositionInput input)
        {
            await CheckPosition(input);
            var position = input.MapTo<Position>();
            await _positionRepository.InsertAsync(position);
        }

        private async Task CheckPosition(CreatePositionInput input)
        {
            var position = await _positionRepository.SingleOrDefaultAsync(p => p.Code == input.Code);
            if (position != null) {
                throw new BusinessException($"系统中已经存在Code为{input.Code}的职位信息");
            }
            var workbookAppServiceProxy = GetService<IWordbookAppService>();
            if (!await workbookAppServiceProxy.Check(new CheckWordbookInput() { WordbookCode = SystemPresetWordbookCode.Organization.PositionFunction, WordbookItemId = input.FunctionId }))
            {
                throw new BusinessException($"系统中不存在指定的岗位职能类型");
            }
            if (!await workbookAppServiceProxy.Check(new CheckWordbookInput() { WordbookCode = SystemPresetWordbookCode.Organization.PositionLevel, WordbookItemId = input.PositionLevelId }))
            {
                throw new BusinessException($"系统中不存在指定的岗位级别");
            }

        }
    }
}
