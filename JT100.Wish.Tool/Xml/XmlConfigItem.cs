using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace JT100.Wish.Tool
{
    public class XmlConfigItem
    {
        public string Key { get; set; }

        public XmlCDataSection Value
        {
            get { return XmlConfigItem.document.CreateCDataSection(this.StringValue); }
            set { this.StringValue = value.Value; }
        }

        private static readonly XmlDocument document = new XmlDocument();

        [XmlIgnore]
        public string StringValue;
    }
}
