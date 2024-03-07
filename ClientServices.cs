using DataAccessLayer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccessLayer.Entities
{
    public class ClientServices : CommonEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Services Services { get; set; }
        public int ServicesId { get; set; }
        public int ClientId { get; set; }
        public bool Status { get; set; }
    }
}