using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class WareInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// Desc:商品标签码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string SNCode { get; set; }

        /// <summary>
        /// Desc:商品编号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string WareCode { get; set; }

        /// <summary>
        /// Desc:商品类型
        /// Default:
        /// Nullable:False
        /// </summary>           
        public byte WareType { get; set; }

        /// <summary>
        /// Desc:状态  0未激活 1闲置中 2出库中 3使用中 4入库中 -1遗失 -2已赔付
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public sbyte Status { get; set; }

        /// <summary>
        /// Desc:客户Id
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? CustomerId { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 商品类型名称
        /// </summary>
        public string WareTypeName { get; set; }
        /// <summary>
        /// 所属用户名称
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// 当前使用客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
		/// 累计清洗次数
		/// </summary>
        public int TotalWashNum { get; set; } = 0;

        /// <summary>
		/// 最近一次发出去的时间
		/// </summary>
        public DateTime? LastSendTime { get; set; }

        /// <summary>
        /// 批次编号
        /// </summary>
        public string GroupNo { get; set; }
    }
}
