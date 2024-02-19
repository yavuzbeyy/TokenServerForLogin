using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Dto
{
    public class ClientLoginDto
    {
        public int ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
