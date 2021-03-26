using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class OutOrder
    {
        /// <summary>
        /// 自增id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 订单状态 1 已出库2 已签收
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 出库单详情
        /// </summary>
        public List<OrderDetail> Details { get; set; }
    }
}
