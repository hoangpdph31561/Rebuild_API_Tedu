using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop_ViewModel.Common
{
    public class PageResult <T>
    {
        public List<T> Items { get; set; }
        public int TotalRecords { get; set; }
    }
}
