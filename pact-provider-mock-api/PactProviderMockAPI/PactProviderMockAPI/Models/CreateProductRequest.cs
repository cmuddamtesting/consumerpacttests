namespace PactProviderMockAPI.Models
{
    public class CreateProductRequest
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double Price { get; set; }

    }
}
