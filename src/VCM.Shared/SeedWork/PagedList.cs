using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.SeedWork
{
    public class PagedList<T>
    {
        public PageHeader PageHeader { get; set; }
        public List<T> Items { set; get; }
        public PagedList() { }
        public PagedList(List<T> items, long count, int pageNumber, int pageSize)
        {
            PageHeader = new PageHeader
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };
            Items = items;
        }
    }
}
