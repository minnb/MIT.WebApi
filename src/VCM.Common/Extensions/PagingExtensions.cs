using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VCM.Common.Extensions
{
    public static class PagingExtensions
    {
        //used by LINQ to SQL
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        //used by LINQ
        public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> Paging<T>(this IQueryable<T> source, int pageNumber, int pageLength)
        {
            return source.Skip(pageNumber * pageLength).Take(pageLength);
        }
    }
}
