using Microsoft.AspNetCore.Mvc;
using PactProviderMockAPI.Models;
using PactProviderMockAPI.Services;
using System.Net;


namespace PactProviderMockAPI.Controllers
{

    [ApiController]
    public class CreateProductController : ControllerBase
    {
        private readonly ICreateProductRepository _createProductRepository;
        public CreateProductController(ICreateProductRepository createProductRepository)
        {
            _createProductRepository = createProductRepository;
        }

        [HttpPost("v1/create")]
        [Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(CreateProductResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody] CreateProductRequest model)
        {
            var existingProductID = await _createProductRepository.FindProductByID(model.ProductID);
            if (existingProductID != null)
            {
                _createProductRepository.DuplicateProduct();
                return Ok(new CreateProductResponse
                {
                    Message = "The Product already exists"
                });
            }
            else
            {
                _createProductRepository.CreateProduct(model);
                return Ok(new CreateProductResponse
                {
                    Message = "Product is created successfully"
                }
                    );
            }
        }
    }
}
