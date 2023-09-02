using EShop_ViewModel.Catalog;
using EShop_ViewModel.Catalog.Manager;
using EShop_ViewModel.Catalog.ProductImages;
using EShop_ViewModel.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop_Appication.Catalog.Product
{
    public interface IManageProductService
    {
        Task<int> Create(ProductCreateRequest request);
        Task<int> Update(ProductUpdateRequest request);
        Task<int> Delete(int productId);
        Task<bool> UpdatePrice(int productId, decimal newPrice);
        Task<bool> UpdateStock(int productId, int addQuantity);
        Task<ProductViewModel> GetProductById(int productId);
        Task AddViewCount(int productId);
        Task<PageResult<ProductViewModel>> GetAllPaging(GetProductPagingRequest request);
        Task<int> AddImages(int productId, ProductImageCreateRequest viewModel);
        Task<int> UpdateImage (int imageId, ProductImageUpdateRequest request);
        Task<int> DeleteImage(int imageId);
        Task<List<ProductImageViewModel>> GetListImages(int productId);
    }
}
