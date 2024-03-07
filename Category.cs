using DataAccessLayer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccessLayer.Entities
{
    public class Category : CommonEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Services Services { get; set; }
		public int ServicesId { get; set; }
        public int ClientId { get; set; }
        public bool Status { get; set; }
        public bool IsShown { get; set; }
        public int Priority { get; set; }
        public int SuperCategoryId { get; set; }
        public string FoodicsId { get; set; }
        public string Image { get; set; }
        public int QuickPOSId { get; set; }
        public int ImageVersion { get; set; }
        public ICollection<CategoryLabels> Labels { get; set; } = new HashSet<CategoryLabels>();
    }
}
