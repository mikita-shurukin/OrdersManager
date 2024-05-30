using System.Text.Json.Serialization;

namespace Manager.Models.Requests
{
    public class CreateCategory
    {
        [JsonIgnore]
        public string? Name { get; set; }
    }
}
