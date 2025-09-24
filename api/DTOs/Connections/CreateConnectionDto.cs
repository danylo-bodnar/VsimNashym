using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Connections
{
    public class CreateConnectionDto
    {
        public long FromTelegramId { get; set; }
        public long ToTelegramId { get; set; }
    }
}