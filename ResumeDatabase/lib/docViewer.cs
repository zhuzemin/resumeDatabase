using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Office.Interop.Word;
using System.Windows.Xps.Packaging;

namespace ResumeDatabase.lib
{
    public class docViewer
    {
        public static XpsDocument ConvertWordDocToXPSDoc(resumeObj resumeObj, string wordDocName, string xpsDocName)
        {
            if (!File.Exists(xpsDocName))
            {
                File.WriteAllBytes(wordDocName,resumeObj.necessaryProp.fileBlob);
                wordToXps(wordDocName, xpsDocName);
            }
            XpsDocument xpsDoc = new XpsDocument(xpsDocName, FileAccess.Read);
            return xpsDoc;
        }

        public static void wordToXps(string wordDocName, string xpsDocName)
        {

            Application wordApplication = new Application();
            wordApplication.Documents.Add(wordDocName);
            Document doc = wordApplication.ActiveDocument;
            doc.SaveAs(xpsDocName, WdSaveFormat.wdFormatXPS);
            wordApplication.Quit();
        }

        public static void ensurePdfFileExist(resumeObj resumeObj,string temporaryFilePath)
        {
            if (!File.Exists(temporaryFilePath))
            {
                File.WriteAllBytes(temporaryFilePath, resumeObj.necessaryProp.fileBlob);
            }
        }
    }
}
