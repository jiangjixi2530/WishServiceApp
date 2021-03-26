using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    [Serializable]
    public class CustomerWare
    {
        public int WareTypeId { get; set; }

        public string WareTypeName { get; set; }

        public int Capacity { get; set; }

        public int Stock { get; set; }

        public decimal LosePrice { get; set; }

        public decimal WashPrice { get; set; }
    }
}
