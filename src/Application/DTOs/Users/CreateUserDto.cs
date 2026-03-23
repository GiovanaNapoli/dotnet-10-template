using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Common;

namespace Application.DTOs.Users
{
    public class CreateUserDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ImageFileDto? ProfilePicture { get; set; }
        public string Password { get; set; } = null!;
    }
}