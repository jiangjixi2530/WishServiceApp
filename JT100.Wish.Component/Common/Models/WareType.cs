using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class WareType
    {
		/// <summary>
		/// 自增ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// 每床数量
		/// </summary>
		public int? BaseCount { get; set; }

		/// <summary>
		/// 遗失单价
		/// </summary>
		public decimal LosePrice { get; set; }

		/// <summary>
		/// 商品类型名称
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// 洗涤单价
		/// </summary>
		public decimal WashPrice { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 修改时间
		/// </summary>
		public DateTime UpdateTime { get; set; }
	}
}
