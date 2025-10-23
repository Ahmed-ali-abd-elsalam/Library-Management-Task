using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Results
{
    public class Error
    {
        public string error { get; set; }
        public Error(string message)
        {
            error = message;
        }
        public static implicit operator Error(string message) => new Error(message);
        public override bool Equals(object? obj)
        {
            if (obj is not Error other) return false;
            return error.Equals(other.error);
        }
    }


    public class Errors
    {
        public static readonly Error WrongPassword = new Error("Enter your password");
        public static readonly Error DoesntExist = new Error("Requested entity doesn't exist");
        public static readonly Error notAvailable =   new Error("requested entity not Available");
        public static readonly Error EmailNotConfirmed = new Error("Email is Not Confirmed");
        public static readonly Error RefreshToken = new Error("Expired Refresh Token please login again");
        public static readonly Error InvalidToken = new Error("Invalid Token please use the correct one");
        public static readonly Error  EmailTaken = new Error("This is Email is Taken");
        public static readonly Error PasswordNotSecure = new Error("Password not Secure it should contain one uppercase,one lower case ,one digit , one special character and 12 characters long");
    }
}