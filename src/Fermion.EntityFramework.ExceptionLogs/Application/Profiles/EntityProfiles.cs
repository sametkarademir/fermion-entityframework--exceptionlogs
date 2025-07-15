using AutoMapper;
using Fermion.EntityFramework.ExceptionLogs.Application.DTOs.ExceptionLogs;
using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;

namespace Fermion.EntityFramework.ExceptionLogs.Application.Profiles;

public class EntityProfiles : Profile
{
    public EntityProfiles()
    {
        CreateMap<ExceptionLog, ExceptionLogResponseDto>();
    }
}