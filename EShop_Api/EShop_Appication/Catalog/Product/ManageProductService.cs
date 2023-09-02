using EShop_Appication.Common;
using EShop_ViewModel.Catalog.Manager;
using EShop_ViewModel.Catalog;
using EShop_ViewModel.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EShop_Data.EF;
using EShop_Data.Entities;
using EShop_Utilities.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using EShop_ViewModel.Catalog.ProductImages;

namespace EShop_Appication.Catalog.Product
{
    public class ManageProductService : IManageProductService
    {
        private readonly EShopDBContext _eShopDBContext;
        private readonly IStorageService _storageService;
        public ManageProductService(EShopDBContext eShopDbContext, IStorageService storageService)
        {
            _eShopDBContext = eShopDbContext;
            _storageService = storageService;
        }

        public async Task<int> AddImages(int productId, ProductImageCreateRequest viewModel)
        {
            var productImage = new ProductImage
            {
                Caption = viewModel.Caption,
                DateCreated = DateTime.Now,
                IsDefault = viewModel.IsDefault,
                ProductId = productId,
                SortOrder = viewModel.SortOrder,
            };
            if(viewModel.ImageFile != null )
            {
                productImage.ImagePath = await this.SaveFile(viewModel.ImageFile);
                productImage.FileSize = viewModel.ImageFile.Length;
            }
            _eShopDBContext.ProductImages.Add(productImage);
            await _eShopDBContext.SaveChangesAsync();
            return productImage.Id;
        }

        public async Task AddViewCount(int productId)
        {
            var product = await _eShopDBContext.Products.FindAsync(productId);
            product.ViewCount += 1;
            await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new EShop_Data.Entities.Product()
            {
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                SeoAlias = request.SeoAlias,
                ProductTranslations = new List<ProductTranslation>()
                { new ProductTranslation()
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Details = request.Details,
                        SeoDescriptions = request.SeoDescription,
                        SeoTitle = request.SeoTitle,
                        SeoAlias = request.SeoAlias,
                        LanguageId = request.LanguageId,
                    }
                }
            };
            //Save image
            if (request.ThumnailImage != null)
            {
                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage()
                    {
                        Caption = "Thumnail Image",
                        FileSize =(int) request.ThumnailImage.Length,
                        ImagePath = await this.SaveFile(request.ThumnailImage),
                        IsDefault = true,
                        SortOrder = 1
                    }
                };
            }
            _eShopDBContext.Products.Add(product);
            return await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<int> Delete(int productId)
        {
            var product = await _eShopDBContext.Products.FindAsync(productId);
            if (product == null)
            {
                throw new EShopException($"Cannot find: {productId}");
            }
            var images = _eShopDBContext.ProductImages.Where(x => x.ProductId == productId);
            foreach (var item in images)
            {
                await _storageService.DeleteFileAsync(item.ImagePath);
            }
            _eShopDBContext.Products.Remove(product);
            return await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<int> DeleteImage(int imageId)
        {
            var image = await _eShopDBContext.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId);    
            if (image == null)
            {
                throw new EShopException($"Cannot find image with id = {imageId}");
            }
            _eShopDBContext.ProductImages.Remove(image);
            return await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<PageResult<ProductViewModel>> GetAllPaging(GetProductPagingRequest request)
        {
            //1. Select join
            var querry = from product in _eShopDBContext.Products
                         join productTrans in _eShopDBContext.ProductTranslations on product.Id equals productTrans.ProductId
                         join productInCategories in _eShopDBContext.ProductInCategories on product.Id equals productInCategories.ProductId
                         join category in _eShopDBContext.Categories on productInCategories.CategoryId equals category.Id

                         select new { product, productTrans, productInCategories };
            //2. filter
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                querry = querry.Where(x => x.productTrans.Name.Contains(request.Keyword));
            }
            if (request.CategoryId.Count > 0)
            {
                querry = querry.Where(x => request.CategoryId.Contains(x.productInCategories.CategoryId));
            }
            //3. Paging
            int totalRow = await querry.CountAsync();
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

        public async Task<List<ProductImageViewModel>> GetListImages(int productId)
        {
            return await _eShopDBContext.ProductImages.Where(x => x.ProductId == productId).Select(x => new ProductImageViewModel
            {
                Caption = x.Caption,
                IsDefault = x.IsDefault,
                DateCreated = x.DateCreated,
                FileSize = x.FileSize,
                Id = x.Id,
                ProductId = x.ProductId,
                ImagePath = x.ImagePath,
                SortOrder = x.SortOrder
            }).ToListAsync();
        }

        public async Task<ProductViewModel> GetProductById(int productId)
        {
            //1. Select join
            var result = await (from product in _eShopDBContext.Products
                         join productTrans in _eShopDBContext.ProductTranslations on product.Id equals productTrans.ProductId
                         where product.Id == productId
                         select new { product, productTrans }).FirstOrDefaultAsync();
            ProductViewModel productView = new ProductViewModel();
            if (result != null)
            {
                productView = new ProductViewModel()
                {
                    Id = result.product.Id,
                    Name = result.productTrans.Name,
                    DateCreated = result.product.DateCreated,
                    Description = result.productTrans.Description,
                    Details = result.productTrans.Details,
                    LanguageId = result.productTrans.LanguageId,
                    OriginalPrice = result.product.OriginalPrice,
                    Price = result.product.Price,
                    SeoAlias = result.product.SeoAlias,
                    SeoDescriptions = result.productTrans.SeoDescriptions,
                    SeoTitle = result.productTrans.SeoTitle,
                    Stock = result.product.Stock,
                    ViewCount = result.product.ViewCount
                };
            }
            else
            {
                productView = null;
            }
            
            return productView;
        }

        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _eShopDBContext.Products.FindAsync(request.Id);
            var productTranslations = await _eShopDBContext.ProductTranslations.FirstOrDefaultAsync(x => x.ProductId == request.Id && x.LanguageId == request.LanguageId);
            if (product == null || productTranslations == null)
            {
                throw new EShopException($"Cannot find: {request.Id}");
            }
            productTranslations.Name = request.Name;
            productTranslations.SeoAlias = request.SeoAlias;
            productTranslations.SeoDescriptions = request.SeoDescriptions;
            productTranslations.SeoTitle = request.SeoTitle;
            productTranslations.Description = request.Description;
            productTranslations.Details = request.Details;
            //Update_anh
            if (request.ThumnailImage != null)
            {
                var thumnailImage = await _eShopDBContext.ProductImages.FirstOrDefaultAsync(x => x.IsDefault == true && x.ProductId == request.Id);
                if (thumnailImage != null)
                {
                    thumnailImage.FileSize = request.ThumnailImage.Length;
                    thumnailImage.ImagePath = await this.SaveFile(request.ThumnailImage);
                    _eShopDBContext.ProductImages.Update(thumnailImage);
                }
            }
            _eShopDBContext.ProductTranslations.Update(productTranslations);
            return await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<int> UpdateImage(int imageId, ProductImageUpdateRequest request)
        {
            var imageUpdate = await _eShopDBContext.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId);
            if (imageUpdate == null)
            {
                throw new EShopException($"Cannot find image with id: {imageId}");
            }
            if (request.ImageFile != null)
            {
                imageUpdate.ImagePath = await this.SaveFile(request.ImageFile);
                imageUpdate.FileSize = request.ImageFile.Length;
            }
            _eShopDBContext.ProductImages.Update(imageUpdate);
            return await _eShopDBContext.SaveChangesAsync();
        }

        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _eShopDBContext.Products.FindAsync(productId);
            if (product == null)
            {
                throw new EShopException($"Cannot find: {productId}");
            }
            product.Price = newPrice;
            _eShopDBContext.Products.Update(product);
            //trả về bool
            return await _eShopDBContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(int productId, int addQuantity)
        {

            var product = await _eShopDBContext.Products.FindAsync(productId);
            if (product == null)
            {
                throw new EShopException($"Cannot find: {productId}");
            }
            product.Stock = addQuantity;
            _eShopDBContext.Products.Update(product);
            //trả về bool
            return await _eShopDBContext.SaveChangesAsync() > 0;
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
    }
}
