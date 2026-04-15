using System;
using System.Collections.Generic;

namespace PrakashCRM.Data.Models
{
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Items = new List<T>();
        }

        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling((double)TotalCount / PageSize);
            }
        }
    }
}