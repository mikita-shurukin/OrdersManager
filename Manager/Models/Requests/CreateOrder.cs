namespace Manager.Models.Requests
{
    public class CreateOrder
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<int> ItemIds { get; set; }
    }
}
