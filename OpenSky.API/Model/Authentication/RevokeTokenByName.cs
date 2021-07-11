namespace OpenSky.API.Model.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class RevokeTokenByName
    {
        public string Name { get; set; }

        public DateTime Expiry { get; set; }
    }
}
