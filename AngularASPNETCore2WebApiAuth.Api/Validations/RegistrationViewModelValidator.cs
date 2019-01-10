using AngularASPNETCore2WebApiAuth.Api.ViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngularASPNETCore2WebApiAuth.Api.Validations
{
    public class RegistrationViewModelValidator : AbstractValidator<RegistrationViewModel>
    {
        public RegistrationViewModelValidator()
        {
            RuleFor(vm => vm.UserName).NotEmpty().WithMessage("Email cannot be empty").EmailAddress().WithMessage("Email is invalid");
            RuleFor(vm => vm.Password).NotEmpty().WithMessage("Password cannot be empty").Equal(q => q.ConfirmPassword).WithMessage("Password must equals confirm password");
            RuleFor(vm => vm.ConfirmPassword).NotEmpty().WithMessage("Confirm password cannot be empty").Equal(q => q.Password).WithMessage("Confirm password must equals password");
            RuleFor(vm => vm.PhoneNumber).NotEmpty().WithMessage("PhoneNumber cannot be empty");
            RuleFor(vm => vm.DisplayName).NotEmpty().WithMessage("Display Name cannot be empty");
        }
    }
}
