using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class OrderDetail
    {
        /// <summary>
        /// 商品类型名称
        /// </summary>
        public string WareTypeName { get; set; }

        /// <summary>
        /// 商品类型ID
        /// </summary>
        public int WareType { get; set; }

        /// <summary>
        /// 商品SN码 逗号分割
        /// </summary>
        public string WareSNCodes { get; set; }
    }
}
