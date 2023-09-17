using PactProviderMockAPI.Models;

namespace PactProviderMockAPI.Services
{
    public class InMemoryCreateProductRepository : ICreateProductRepository
    {
        private static readonly List<CreateProductRequest> createProductRequestModels =
            new List<CreateProductRequest> { };
        public Task<CreateProductRequest> FindProductByID(int productID)
        {
            return Task.FromResult(createProductRequestModels.Find(
                st => st.ProductID == productID));
        }
        public void CreateProduct(CreateProductRequest createProductRequest)
        {
            createProductRequestModels.Add(createProductRequest);
        }
        public void DuplicateProduct()
        {
            createProductRequestModels.Clear();
        }
    }
}
