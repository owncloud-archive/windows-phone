using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using OwnCloud.Resource;

namespace OwnCloud.Data
{
    class MimeTypes
    {
        static Dictionary<string, string> _types;

        /// <summary>
        /// Returns the Name for the MIME type if exists. If the complete MIME declaration is missing for the given language 
        /// it will return the "en"-values. Otherwise it
        /// will return the string for "File_Type_Default"
        /// </summary>
        /// <param name="type">A MIME type</param>
        /// <returns>Translated Name of the MIME type regarding the current language</returns>
        static public string GetNameOf(string type)
        {
            if (_types == null)
            {
                _types = new Dictionary<string, string>();
                Stream stream = null;

                try
                {
                    stream = ResourceLoader.GetStream(String.Format("/Resource/Localization/Mime.{0:g}.xml", CultureInfo.CurrentCulture.ToString().Substring(0, 2)));
                }
                catch (Exception)
                {
                    stream = ResourceLoader.GetStream("/Resource/Localization/Mime.en.xml");
                }

                try
                {
                    XDocument doc = XDocument.Load(stream);
                    foreach (var el in doc.Element("mimelist").Elements("mime"))
                    {
                        string key = el.Attribute("type").Value.ToLower();
                        if (!_types.ContainsKey(key))
                        {
                            _types.Add(el.Attribute("type").Value, el.Value);
                        }
                    }
                }
                catch (Exception)
                {
                    
                }
            }

            try
            {
                return _types[type.ToLower()];
            }
            catch (Exception)
            {
                return LocalizedStrings.Get("File_Type_Default");
            }
        }
    }
}
