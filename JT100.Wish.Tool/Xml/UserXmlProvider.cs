using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Xml;
using System.Xml.Serialization;

namespace JT100.Wish.Tool
{
    public class UserXmlProvider
    {
        private string _userConfigPath = AppDomain.CurrentDomain.BaseDirectory + "\\Users\\Config\\UserConfig.xml";

        private string _configPath = AppDomain.CurrentDomain.BaseDirectory + "\\Users\\Config\\Config.xml";

        private Dictionary<string, XmlConfigItem> _xmlConfigDic;
        public UserXmlProvider()
        {
            Initalize();
        }
        private UserXmlProvider(string path)
        {
            _userConfigPath = path;
            Initalize();
        }

        private void Initalize()
        {
            try
            {
                var directory = Path.GetDirectoryName(_userConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (!File.Exists(_userConfigPath))
                {
                    File.Create(_userConfigPath);
                }
                string content = File.ReadAllText(_userConfigPath);
                if (string.IsNullOrEmpty(content))
                {
                    _xmlConfigDic = this.Deserialize<List<XmlConfigItem>>(content).ToDictionary(_ => _.Key);
                }
                else
                {
                    _xmlConfigDic = new Dictionary<string, XmlConfigItem>();
                }
            }
            catch (Exception)
            {
                _xmlConfigDic = _xmlConfigDic ?? new Dictionary<string, XmlConfigItem>();
            }
        }

        public T GetConfig<T>(string key)
        {
            if (_xmlConfigDic.ContainsKey(key))
            {
                return this.Deserialize<T>(_xmlConfigDic[key].StringValue);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 获取系统模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetSysConfig<T>()
        {
            string content = File.ReadAllText(_configPath);
            return Deserialize<T>(content);

        }

        public void SetConfig<T>(string key, T t)
        {
            if (_xmlConfigDic.ContainsKey(key))
            {
                var item = _xmlConfigDic[key];
                if (item == null)
                {
                    item = new XmlConfigItem();
                    item.Key = key;
                }
                item.StringValue = this.Serialize<T>(t);
            }
            else
            {
                XmlConfigItem xmlConfigItem = new XmlConfigItem
                {
                    Key = key,
                    StringValue = this.Serialize<T>(t)
                };
                _xmlConfigDic.Add(key, xmlConfigItem);
            }
            List<XmlConfigItem> xmlConfigs = _xmlConfigDic.Values.ToList();
            try
            {
                File.WriteAllText(_userConfigPath, this.Serialize<List<XmlConfigItem>>(xmlConfigs), Encoding.UTF8);
            }
            catch (Exception)
            {

            }
        }

        public void SetSysConfig<T>(T t)
        {
            try
            {
                File.WriteAllText(_configPath, this.Serialize<T>(t), Encoding.UTF8);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 对象序列化成 XML String
        /// </summary>
        private string Serialize<T>(T obj)
        {
            string xmlString = string.Empty;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    xmlSerializer.Serialize(ms, obj);
                    xmlString = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception)
            {
                xmlString = string.Empty;
            }
            return xmlString;
        }
        /// <summary>
        /// XML String 反序列化成对象
        /// </summary>
        private T Deserialize<T>(string xmlString)
        {
            T t = default(T);
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
                {
                    using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                    {
                        Object obj = xmlSerializer.Deserialize(xmlReader);
                        t = (T)obj;
                    }
                }
            }
            catch (Exception)
            {
                t = default(T);
            }
            return t;
        }
    }
}
