using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    [Serializable]
    public class PageModel<T>
    {
        /// <summary>
        /// 当前页标
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; } = 6;
        /// <summary>
        /// 数据总数
        /// </summary>
        public long DataCount { get; set; } = 0;
        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { set; get; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public List<T> Data { get; set; }
    }
}
