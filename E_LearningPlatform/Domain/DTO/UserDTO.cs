using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class UserDTO
    {
        public int id { get; set; }
        public string? fullName { get; set; }
        public string? phoneNumber { get; set; }
        public string? email { get; set; }
        public string? gender { get; set; }

        public string? Address { get; set; }

        public long? nationalId { get; set; }
    }
}
