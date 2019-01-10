using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.ViewModels.Mappings
{
    public class ProfileStartupFilter : IStartupFilter
    {
        private IMapper _mapper;

        public ProfileStartupFilter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                _mapper.ConfigurationProvider.AssertConfigurationIsValid();
                next(builder);
            };
        }
    }
}
