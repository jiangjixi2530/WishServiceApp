using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    [Serializable]
    public class CustomModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Desc:名称
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Name { get; set; }

        /// <summary>
        /// Desc:性别 1男 2女
        /// Default:1
        /// Nullable:False
        /// </summary>           
        public byte Sex { get; set; }

        /// <summary>
        /// Desc:联系方式
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Phone { get; set; }

        /// <summary>
        /// Desc:门店名称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string StoreName { get; set; }

        /// <summary>
        /// Desc:门店位置
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string StoreAddress { get; set; }

        /// <summary>
        /// Desc:门店联系方式
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string StorePhone { get; set; }

        /// <summary>
        /// Desc:门店类型 1酒店 0其他
        /// Default:
        /// Nullable:False
        /// </summary>           
        public byte StoreType { get; set; }

        public int OwnerId { get; set; }


        public string OwnerName { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Balance { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }
        public List<CustomerWare> WareInfos { get; set; }
    }
}
