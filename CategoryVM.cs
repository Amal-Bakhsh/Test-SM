using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class CategoryVM
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public bool IsShown { get; set; }
        public int Priority { get; set; }
        public int ServicesId { get; set; }
        public int SuperCategoryId { get; set; }
        public string FoodicsId { get; set; }
        public string Image { get; set; }
        public int QuickPOSId { get; set; }
        public int ImageVersion { get; set; }
        public List<CategoryLabelsVM> Labels { get; set; } = new List<CategoryLabelsVM>();
    }
}
