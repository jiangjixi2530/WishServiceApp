using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace JT100.Wish.Component.Common.Models
{
    [XmlRoot("Config")]
    public class SysConfig
    {
        [XmlAttribute("ServerUrl")]
        public string ServerUrl { get; set; } 
    }
}
