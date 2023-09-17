using PactProviderMockAPI.Models;

namespace PactProviderMockAPI.Services
{
    public interface ICreateProductRepository
    {
        public void CreateProduct(CreateProductRequest createProductRequest);
        public Task<CreateProductRequest> FindProductByID(int productID);
        public void DuplicateProduct();
    }
}
