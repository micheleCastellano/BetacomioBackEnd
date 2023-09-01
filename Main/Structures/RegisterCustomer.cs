using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Main.Structures
{
    public class RegisterCustomer
    {
        public string? firstName { get; set; } 
        public string? lastName { get; set; } 
        public string? emailAddress { get; set; }
        public string? phone { get; set; }
        public string? password { get; set; }

    }
}