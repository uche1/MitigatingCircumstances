﻿using Microsoft.AspNetCore.Identity;

namespace MitigatingCircumstances.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Fullname => $"{Firstname} {Lastname}";

        public bool IsTutor => false;

        public bool IsStudent => false;
    }
}
