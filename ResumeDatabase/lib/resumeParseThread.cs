using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace ResumeDatabase.lib
{
    class resumeParseThread
    {
        //background process initialize
        public BackgroundWorker m_BackgroundWorker;// 申明后台对象
        public ManualResetEvent _busy = new ManualResetEvent(true);
        bool paused = false;
        MainWindow parent;
        Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();

        public resumeParseThread(MainWindow mainWindow)
        {
            parent = mainWindow;
            //background process initialize
            m_BackgroundWorker = new BackgroundWorker(); // 实例化后台对象
            m_BackgroundWorker.WorkerReportsProgress = true; // 设置可以通告进度
            m_BackgroundWorker.WorkerSupportsCancellation = true; // 设置可以取消
            m_BackgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
            m_BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgress);
            m_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedWork);
        }

        void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            parent.progressBar.Value = e.ProgressPercentage;
            var resumeObj = e.UserState as resumeObj;
            parent.textBlock_progressBarStatus.Text = string.Format("process: {0}", resumeObj.necessaryProp.filePath);
            parent.mySqlUtils.insertToRecordedFileTable(resumeObj);
            if (resumeObj.error)
            {
                parent.errorResumeList.Add(resumeObj);
                parent.mySqlUtils.insert(resumeObj, parent.mStr.dbErrorResumeTableName);
            }
            else
            {
                parent.mySqlUtils.insert(resumeObj, parent.mStr.dbResumeTableName);
            }
        }

        void CompletedWork(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show("Error: " + e.Error);
            }
            else if (e.Cancelled)
            {
                System.Windows.MessageBox.Show("Canceled");
            }
            else
            {
                System.Windows.MessageBox.Show("Import complete");
                parent.button_controlStart.IsEnabled = true;
            }
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> unrecordedResumeFilePathList = (List < string > )e.Argument;
            BackgroundWorker bw = sender as BackgroundWorker;
            int count = 1;
            foreach(string filePath in unrecordedResumeFilePathList)
            {
                _busy.WaitOne();
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                string fileContent="";
                if (filePath.Contains(".docx")||filePath.Contains(".doc"))
                {
                    fileContent = GetTextFromWord(filePath);
                }
                else if (filePath.Contains(".pdf"))
                {
                    fileContent=ExtractTextFromPDF(filePath);
                }
                //Debug.Write("fileContent: " + fileContent);
                if (fileContent != "")
                {
                    resumeObj resumeObj = infoExtract.get(fileContent,parent.mStr.skillDict);
                    resumeObj.necessaryProp.filePath = filePath;
                    resumeObj.necessaryProp.fileHash = fileUtils.CalculateMD5(filePath);
                    resumeObj.necessaryProp.fileBlob = File.ReadAllBytes(filePath);
                    parent.Dispatcher.Invoke(() =>
                    {
                        resumeObj.necessaryProp.logDate = parent.textBox_dbLogDate.Text;
                    });
                if (filePath.Contains(".docx") || filePath.Contains(".doc"))
                {
                        string fileName = resumeObj.necessaryProp.filePath.Split('\\').Last();
                        resumeObj.necessaryProp.name = Regex.Match(fileName, "([\u4e00-\u9fa5]{2,4})").Groups[1].Value;
                    infoExtractUseXml.get(resumeObj, parent.mStr);
                }
                foreach (System.Reflection.PropertyInfo prop in resumeObj.necessaryProp.GetType().GetProperties())
                {
                    var value=prop.GetValue(resumeObj.necessaryProp);
                    if (value == null)
                    {
                        Debug.Write("\nresumeObj.name: " + prop.Name);
                        resumeObj.error = true;
                    }
                }
                        if(resumeObj.necessaryProp.phone!=null)
                        {
                        resumeObj.necessaryProp.phone = resumeObj.necessaryProp.phone.Replace("-", "");
                        resumeObj.necessaryProp.phone = resumeObj.necessaryProp.phone.Replace("－", "");
                        foreach (string keyword in parent.mStr.overseaJobKeywordList)
                        {
                            if (resumeObj.necessaryProp.filePath.Contains(keyword) || resumeObj.necessaryProp.phone.Contains(keyword))
                            {
                                resumeObj.optionProp.overseaJob = parent.mStr.overseaJobState_intended;
                                break;
                            }
                        }
                        }

                    bw.ReportProgress(count++, resumeObj);
                }
            }
            word.Quit();
        }

        public static string ExtractTextFromPDF(string filePath)
        {
            PdfReader pdfReader = new PdfReader(filePath);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            string pageContent="";
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                pageContent += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
            }
            pdfDoc.Close();
            pdfReader.Close();
            return pageContent;
        }

        /// <summary>  
        /// Reading Text from Word document  
        /// </summary>  
        /// <returns></returns>  
        private string GetTextFromWord(string filePath)
        {
            StringBuilder text = new StringBuilder();
            object miss = System.Reflection.Missing.Value;
            object path = filePath;
            object readOnly = true;
            Microsoft.Office.Interop.Word.Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);

            for (int i = 0; i < docs.Paragraphs.Count; i++)
            {
                text.Append(docs.Paragraphs[i + 1].Range.Text.ToString());
            }
            return text.ToString();
        }
    }
}
