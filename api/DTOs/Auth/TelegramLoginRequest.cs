using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs.Auth
{
    public class TelegramLoginRequest
    {
        public string InitData { get; set; } = string.Empty;
    }
}