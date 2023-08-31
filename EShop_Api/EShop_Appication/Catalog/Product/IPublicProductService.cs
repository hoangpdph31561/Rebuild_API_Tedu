using EShop_ViewModel.Catalog.Public;
using EShop_ViewModel.Catalog;
using EShop_ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop_Appication.Catalog.Product
{
    public interface IPublicProductService
    {
        Task<List<ProductViewModel>> GetAll();
        Task<PageResult<ProductViewModel>> GetAllByCategoryId(GetProductPagingRequestPublic request);
    }
}
