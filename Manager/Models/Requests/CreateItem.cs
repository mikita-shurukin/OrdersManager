namespace Manager.Models.Requests
{
    public class CreateItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
    }
}
