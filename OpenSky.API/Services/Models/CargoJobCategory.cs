namespace OpenSky.API.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CargoJobCategory
    {
        public int Payload { get; set; }

        public int MinDistance { get; set; }

        public int MaxDistance { get; set; }

        public double PaymentPerNM { get; set; }
    }
}
