using DataAccessLayer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccessLayer.Entities
{
    public class Services : CommonEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Client Client { get; set; }
        public int ClientId { get; set; }
        public bool IsDisable { get; set; }
        public bool Status { get; set; }
        public bool IsShown { get; set; }
        public int Priority { get; set; }
        public int SuperServicesId { get; set; }
        public string Image { get; set; }
        public ICollection<ServicesLabels> Labels { get; set; } = new HashSet<ServicesLabels>();
    }
}
