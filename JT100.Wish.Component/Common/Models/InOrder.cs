using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class InOrder
    {
        /// <summary>
        /// 自增id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 客户名词
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 酒店名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单状态 1 已出库2 已签收
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 入库类型 1 日常入库 2 反洗入库
        /// </summary>
        public int InType { get; set; }

        /// <summary>
        /// 如库单详情
        /// </summary>
        public List<OrderDetail> Details { get; set; }
    }
}
