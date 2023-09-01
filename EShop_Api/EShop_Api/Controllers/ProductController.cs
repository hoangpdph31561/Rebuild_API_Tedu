using EShop_Appication.Catalog.Product;
using EShop_ViewModel.Catalog.Manager;
using EShop_ViewModel.Catalog.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EShop_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IPublicProductService _publicProductService;
        private readonly IManageProductService _manageProductService;
        public ProductController(IPublicProductService publicProductService, IManageProductService manageProductService)
        {
            _publicProductService = publicProductService;
            _manageProductService = manageProductService;

        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _publicProductService.GetAll();
            return Ok(result);
        }
        [HttpGet("public-paging")]
        public async Task<IActionResult> GetProductById([FromQuery]GetProductPagingRequestPublic request)
        {
            try
            {
                var product = await _publicProductService.GetAllByCategoryId(request);
                if(product == null)
                {
                    return NotFound($"Cannot find your product with {request.CategoryId}");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _manageProductService.GetProductById(id);
                if(result == null)
                {
                    return NotFound($"Cannot find product with Id: {id}");
                }
                return Ok(result);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewProduct([FromForm]ProductCreateRequest request)
        {
            try
            {
                int result = await _manageProductService.Create(request);
                if( result == 0 )
                {
                    return BadRequest("Cannot create new product");
                }
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromForm]ProductUpdateRequest request)
        {
            try
            {
                var update = await _manageProductService.Update(request);
                if (update == 0)
                {
                    return BadRequest("Unable to update");
                }
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _manageProductService.Delete(id);
                if(result == 0 )
                {
                    return BadRequest("Unable to delete");
                }
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
        [HttpPut("prict/{id}/{newPrice}")]
        public async Task<IActionResult> UpdatePrice(int id, decimal newPrice)
        {
            try
            {
                bool result = (bool) await _manageProductService.UpdatePrice(id, newPrice);
                if(!result)
                {
                    return NotFound("Cannot update price");
                }
                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
