using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class ServicesVM
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public bool IsDisable { get; set; }
        public bool Status { get; set; }
        public bool IsShown { get; set; }
        public string Image { get; set; }
        public int Priority { get; set; }
        public int SuperServicesId { get; set; }
        public List<ServicesLabelsVM> Labels { get; set; } = new List<ServicesLabelsVM>();

    }
}
