using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Models
{
    public class PropertiesModel : List<PropertyModel>
    {
        public PropertiesModel()
        {
        }

        public PropertiesModel(IEnumerable<PropertyModel> properties) : base(properties)
        {
        }

        public void Add(string parameter, string description, string value = "")
        {
            base.Add(new PropertyModel()
            {
                Parameter = parameter,
                Description = description,
                Value = value
            });
        }
    }

    public class PropertyModel
    {
        public string Parameter { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }
}
