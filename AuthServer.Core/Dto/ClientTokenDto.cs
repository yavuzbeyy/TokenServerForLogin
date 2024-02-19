using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Dto
{
    public class ClientTokenDto
    {
        public string AccessToken { get; set; }

        public DateTime AccesTokenExpiration { get; set; }
    }
}
