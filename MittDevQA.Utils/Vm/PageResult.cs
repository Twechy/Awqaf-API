using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Vm
{
    public class PageResult<T>
    {
        public int NumberOfPages { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public List<T> PageContent { get; set; }

        public static PageResult<T> Create(int totalCount, int pageNumber, int pageSize, List<T> content)
            => new PageResult<T>
            {
                PageContent = content,
                PageNumber = pageNumber,
                NumberOfPages = (int)Math.Ceiling((double)totalCount / pageSize),
                PageSize = pageSize
            };

        private PageResult()
        {
        }
    }

    public static class PaginateExtension
    {
        public static IEnumerable<T> ToPageResult<T>(this IEnumerable<T> query, int page = 1, int pageSize = 20)
        {
            return query.Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}