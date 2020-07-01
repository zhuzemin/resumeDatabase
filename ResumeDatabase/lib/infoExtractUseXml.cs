using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ResumeDatabase.lib
{
    class infoExtractUseXml
    {
        public static void get( resumeObj resumeObj, magicStringObj mStr)
        {
            try
            {
            XmlDocument doc = new XmlDocument();
            doc.Load(resumeObj.necessaryProp.filePath);
            XmlNodeList nodeList=  doc.GetElementsByTagName("w:t");
            foreach(XmlNode node in nodeList)
            {
                Debug.Write("node.InnerText: "+node.InnerText);
                    Debug.Write("resumeObj.necessaryProp.name: " + resumeObj.necessaryProp.name);
                    Match matchResult = Regex.Match(node.InnerText, "([\u4e00-\u9fa5]{2,4})");
                    if (resumeObj.necessaryProp.name==null&& matchResult.Success &&resumeObj.necessaryProp.filePath.Contains(matchResult.Groups[1].Value) && !mStr.resumeFilenameExcludeKeywordList.Contains(matchResult.Groups[1].Value))
                    {
                        resumeObj.necessaryProp.name = node.InnerText;
                    }
            }
                Debug.Write(doc.GetElementsByTagName("w:binData")[0].InnerText);
                //for boss zhipin
                if (doc.GetElementsByTagName("w:binData")[0].InnerText == mStr.bossZhipinCharacteristic)
                {
                    resumeObj.necessaryProp.region = "null";
                    resumeObj.necessaryProp.mail = "null";
                }
            }
            catch(Exception e)
            {
                Debug.Write(e);
            }
        }
    }
}
