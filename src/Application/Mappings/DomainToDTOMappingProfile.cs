using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities.Common;
using Domain.Entities.Users;

namespace Application.Mappings
{
    public class DomainToDTOMappingProfile : Profile
    {
        public DomainToDTOMappingProfile()
        {
            CreateMap<User, UsersDto>().ReverseMap();
            CreateMap<CreateUserDto, User>().ReverseMap();
            CreateMap<ImageFile, ImageFileDto>().ReverseMap();
        }
    }
}