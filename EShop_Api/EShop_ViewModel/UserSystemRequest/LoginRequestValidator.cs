using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop_ViewModel.UserSystemRequest
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserNmae is required");
            RuleFor(x => x.Passwrod).NotEmpty().WithMessage("PassWord is required").MinimumLength(6).WithMessage("PassWord must be at least 6 characters");

        }
    }
}
