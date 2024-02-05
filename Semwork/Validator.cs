using FluentValidation;
using Semwork.Models;

namespace Semwork;

public class Validator: AbstractValidator<RegUser>
{
    public Validator(bool isRepeatNick = false, bool isRepeatEmail = false, bool isRepeatNumber = false)
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is empty")
            .Must(y => y.Contains(' ')).WithMessage("Write your full name");

        if (!isRepeatEmail)
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("This is not email")
                .NotEmpty().WithMessage("Email is empty");
        } else {
            RuleFor(x => x.Email).Must(_ => !isRepeatEmail).WithMessage("This email is busy");
        }
        
        if (!isRepeatNumber)
        {
            RuleFor(x => x.Number).NotEmpty().WithMessage("Phone number is empty")
                .Must(y => y.All(char.IsDigit)).WithMessage("There aren't numbers in phone number")
                .Length(11).WithMessage("Phone number should has 11 numbers");
        }
        else
        {
            RuleFor(x => x.Number).Must(_ => !isRepeatNumber).WithMessage("Phone number is busy");
        }

        if (!isRepeatNick)
        {
            RuleFor(x => x.Nick).NotEmpty().WithMessage("Nick is empty")
                .Length(2, 60).WithMessage("Nick should has length from 2 to 60");
        }
        else
        {
            RuleFor(x => x.Nick).Must(_ => !isRepeatNick).WithMessage("Nick is busy");
        }

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is empty")
            .Length(6, 40).WithMessage("Password should has length from 6 to 40")
            .Must(y => ContainsAny(y, @"!\#$%&'()*+,-./:;<=>?@[]^_`{|}~"))
            .WithMessage("Password should has special symbols");
    }
    
    public string[] ValidateAndGetErrorMessages(RegUser user)
    {
        var valRes = Validate(user);
        if (valRes.Errors.Count == 0)
            return new string[0];

        return valRes.Errors
            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
            .OrderBy(x => x.Split(':')[0])
            .ToArray();
    }
    
    private bool ContainsAny(string str, IEnumerable<char> symbols)
    {
        return symbols.Any(c => str.Contains(c));
    }
}