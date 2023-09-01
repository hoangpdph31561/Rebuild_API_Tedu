using EShop_Data.EF;
using EShop_ViewModel.Catalog.Public;
using EShop_ViewModel.Catalog;
using EShop_ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EShop_Appication.Catalog.Product
{
    public class PublicProductService : IPublicProductService
    {
        private readonly EShopDBContext _eShopDBContext;
        public PublicProductService(EShopDBContext eShopDBContext)
        {
            _eShopDBContext = eShopDBContext;
        }
        public async Task<List<ProductViewModel>> GetAll()
        {
            var querry = from product in _eShopDBContext.Products
                         join productTrans in _eShopDBContext.ProductTranslations on product.Id equals productTrans.ProductId
                         
                         select new { product, productTrans};
            int totalRow = await querry.CountAsync(); //lấy dòng để phân trang
            var data = await querry.Select(x => new ProductViewModel
            {
                Id = x.product.Id,
                Name = x.productTrans.Name,
                DateCreated = x.product.DateCreated,
                Description = x.productTrans.Description,
                Details = x.productTrans.Details,
                LanguageId = x.productTrans.LanguageId,
                OriginalPrice = x.product.OriginalPrice,
                Price = x.product.Price,
                SeoAlias = x.product.SeoAlias,
                SeoDescriptions = x.productTrans.SeoDescriptions,
                SeoTitle = x.productTrans.SeoTitle,
                Stock = x.product.Stock,
                ViewCount = x.product.ViewCount

            }).ToListAsync();

            return data;
        }



        public async Task<PageResult<ProductViewModel>> GetAllByCategoryId(GetProductPagingRequestPublic request)
        {
            //1. Select join
            var querry = from product in _eShopDBContext.Products
                         join productTrans in _eShopDBContext.ProductTranslations on product.Id equals productTrans.ProductId
                         join productInCategories in _eShopDBContext.ProductInCategories on product.Id equals productInCategories.ProductId
                         join category in _eShopDBContext.Categories on productInCategories.CategoryId equals category.Id

                         select new { product, productTrans, productInCategories };
            //2. filter
            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
            {
                querry = querry.Where(x => x.productInCategories.CategoryId == request.CategoryId);
            }
            //3. Paging
            int totalRow = await querry.CountAsync(); //lấy dòng để phân trang
            var data = querry.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).Select(x => new ProductViewModel
            {
                Id = x.product.Id,
                Name = x.productTrans.Name,
                DateCreated = x.product.DateCreated,
                Description = x.productTrans.Description,
                Details = x.productTrans.Details,
                LanguageId = x.productTrans.LanguageId,
                OriginalPrice = x.product.OriginalPrice,
                Price = x.product.Price,
                SeoAlias = x.product.SeoAlias,
                SeoDescriptions = x.productTrans.SeoDescriptions,
                SeoTitle = x.productTrans.SeoTitle,
                Stock = x.product.Stock,
                ViewCount = x.product.ViewCount

            });
            //4. Select and project
            var productResult = new PageResult<ProductViewModel>()
            {
                TotalRecords = totalRow,
                Items = await data.ToListAsync()
            };
            return productResult;
        }
    }
}
