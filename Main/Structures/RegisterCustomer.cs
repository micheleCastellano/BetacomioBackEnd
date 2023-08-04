using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Main.Structures
{
    public class RegisterCustomer
    {
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }

    }
}