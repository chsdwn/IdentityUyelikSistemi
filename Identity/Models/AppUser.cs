using System;
using Microsoft.AspNetCore.Identity;

namespace Identity.Models
{
    public class AppUser : IdentityUser
    {
        public string City { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}