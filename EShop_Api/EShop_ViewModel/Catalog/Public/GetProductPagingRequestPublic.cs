using EShop_ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop_ViewModel.Catalog.Public
{
    public class GetProductPagingRequestPublic : PagingRequestBase
    {
        public int? CategoryId { get; set; }
    }
}
