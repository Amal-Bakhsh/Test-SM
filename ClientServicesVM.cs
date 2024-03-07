using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class ClientServicesVM
    {
        public int Id { get; set; }
        public ServicesVM Services { get; set; }
        public int ServicesId { get; set; }
        public int ClientId { get; set; }
        public bool Status { get; set; }
    }
}
