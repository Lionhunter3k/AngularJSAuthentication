using AngularASPNETCore2WebApiAuth.Api.ViewModels;
using AutoMapper;
using nH.Identity.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Mappings
{
    public class RegistrationProfile : Profile
    {
        public RegistrationProfile()
        {
            CreateMap<RegistrationViewModel, User>(MemberList.Source)
                .ForMember(au => au.Email, map => map.MapFrom(vm => vm.UserName))
                .ForSourceMember(au => au.Password, map => map.DoNotValidate())
                .ForSourceMember(au => au.ConfirmPassword, map => map.DoNotValidate());
        }
    }
}
