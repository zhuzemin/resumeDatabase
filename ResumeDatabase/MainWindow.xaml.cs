using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using log4net;
using System.Diagnostics;
using ResumeDatabase.lib;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace ResumeDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        configObj cfg = new configObj();
        public magicStringObj mStr = new magicStringObj();
        public MySqlConnection conn;
        public List<string> unrecordedResumeFilePathList = new List<string>();
        public mySqlUtils mySqlUtils;
        resumeParseThread resumeParseThread;
        public ObservableCollection<resumeObj> errorResumeList = new ObservableCollection<resumeObj>();
        public ObservableCollection<resumeObj> queryRersultList = new ObservableCollection<resumeObj>();
        bool threadPasused = false;
        resumeObj currentResumeObj = new resumeObj();

        public MainWindow()
        {
            InitializeComponent();

            //config store initialize
            textBox_dbServer.Text = cfg.dbServer;
            textBox_dbDatabase.Text = cfg.dbDatabase;
            textBox_dbUser.Text = cfg.dbUser;
            textBox_dbPasswd.Text = cfg.dbPasswd;
            textBox_dbLogDate.Text = cfg.dbLogDate;
            textBox_resumeFolderPath.Text = cfg.resumeFolderPath;

            //log4net
            AppDomain currentDomain = default(AppDomain);
            currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            // Handler for exceptions in threads behind forms.
            System.Windows.Forms.Application.ThreadException += GlobalThreadExceptionHandler;

            //control initial status
            groupBox_control.IsEnabled = false;
            groupBox_controlInvisible.IsEnabled = false;
            textBox_dbLogDate.IsEnabled = false;
            if (textBox_resumeFolderPath.Text != "")
            {
                button_resumeFolderPathLoad.IsEnabled = true;
            }
            else
            {
                button_resumeFolderPathLoad.IsEnabled = false;
            }
            new textBoxPlaceholder(textBox_dbLogDate, textBox_dbLogDate.Text);
            button_query.IsEnabled = false;
            menu_skill.Visibility = Visibility.Hidden;
            grid_skill_initial(3);
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            resumeObj resumeObj = (resumeObj)listView.SelectedItem;
            currentResumeObj = resumeObj;
            if (resumeObj != null)
            {
                resumeObj.isEnabled = true;
                foreach (var obj in errorResumeList)
                {
                    if (obj != resumeObj)
                    {
                        obj.isEnabled = false;
                    }
                }
                foreach (var obj in queryRersultList)
                {
                    if (obj != resumeObj)
                    {
                        obj.isEnabled = false;
                    }
                }
                listView.Items.Refresh();

                if (resumeObj.necessaryProp.filePath.Contains(".doc"))
                {
                    documentViewer.Visibility = Visibility.Visible;
                    WebBrowser.Visibility = Visibility.Hidden;
                    docViewer.wordToXps(resumeObj.necessaryProp.filePath, mStr.tempFolderName + @"\" + resumeObj.necessaryProp.fileHash + ".xps");
                    string fileExtension = resumeObj.necessaryProp.filePath.Split('.').Last();
                    documentViewer.Document = docViewer.ConvertWordDocToXPSDoc(resumeObj, mStr.tempFolderName + @"\" + resumeObj.necessaryProp.fileHash + "." + fileExtension, mStr.tempFolderName + @"\" + resumeObj.necessaryProp.fileHash + ".xps").GetFixedDocumentSequence();
                }
                else if (resumeObj.necessaryProp.filePath.Contains(".pdf"))
                {
                    string temporaryFilePath = mStr.tempFolderName + @"\" + resumeObj.necessaryProp.fileHash + ".pdf";
                    docViewer.ensurePdfFileExist(resumeObj, temporaryFilePath);
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = temporaryFilePath;
                    process.Start();
                }
            }
        }

        private void button_resumeFolderPathBrowser_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox_resumeFolderPath.Text = fbd.SelectedPath;
                    button_resumeFolderPathLoad.IsEnabled = true;
                    button_resumeFolderPathLoad_Click(sender, e);
                }
            }
        }

        private void button_dbConnect_Click(object sender, RoutedEventArgs e)
        {
            string connetionString = null;
            connetionString = "server=" + textBox_dbServer.Text + ";database=" + textBox_dbDatabase.Text + ";uid=" + textBox_dbUser.Text + ";pwd=" + textBox_dbPasswd.Text + ";CharSet=utf8;Allow User Variables=True;";
            conn = new MySqlConnection(connetionString);
            try
            {
                conn.Open();
                Debug.Write("Connection Open ! ");
                mySqlUtils = new mySqlUtils(this);
                mySqlUtils.ensureTableExist(mStr.dbRecordedFileTableName);
                mySqlUtils.ensureTableExist(mStr.dbErrorResumeTableName);
                mySqlUtils.ensureTableExist(mStr.dbResumeTableName);
                groupBox_control.IsEnabled = true;
                textBlock_dbStatus.Text = mStr.dbConnSuc;
                string statement = "SELECT * FROM " + mStr.dbErrorResumeTableName;
                errorResumeList = mySqlUtils.selectFromTable(statement);
                listView_errorResume.ItemsSource = errorResumeList;
                button_query.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.Write("Can not open connection ! ");
                textBlock_dbStatus.Text = mStr.dbConnFailed;
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void button_controlStart_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Maximum = unrecordedResumeFilePathList.Count;
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            resumeParseThread = new resumeParseThread(this);
            resumeParseThread.m_BackgroundWorker.RunWorkerAsync(unrecordedResumeFilePathList);
            btn.IsEnabled = false;
        }

        private void button_controlPause_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            Debug.Write("btn.Content: " + btn.Content);
            if (threadPasused)
            {
                resumeParseThread._busy.Set();
                threadPasused = false;
                btn.Content = mStr.button_controlPause_Pause;
            }
            else
            {
                resumeParseThread._busy.Reset();
                threadPasused = true;
                btn.Content = mStr.button_controlPause_Resume;
            }
        }

        private void button_controlCancel_Click(object sender, RoutedEventArgs e)
        {
            resumeParseThread.m_BackgroundWorker.CancelAsync();
            button_controlStart.IsEnabled = true;
        }

        private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = (Exception)e.ExceptionObject;
            ILog log = LogManager.GetLogger(typeof(MainWindow));
            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

        private static void GlobalThreadExceptionHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = e.Exception;
            ILog log = LogManager.GetLogger(typeof(MainWindow)); //Log4NET
            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            conn.Close();
            //config saving
            cfg.dbServer = textBox_dbServer.Text;
            cfg.dbDatabase = textBox_dbDatabase.Text;
            cfg.dbUser = textBox_dbUser.Text;
            cfg.dbPasswd = textBox_dbPasswd.Text;
            cfg.dbLogDate = textBox_dbLogDate.Text;
            cfg.resumeFolderPath = textBox_resumeFolderPath.Text;
            cfg.save();
        }

        private void button_resumeFolderPathLoad_Click(object sender, RoutedEventArgs e)
        {
            textBox_dbLogDate.IsEnabled = true;
            List<string> filePathList = fileUtils.walk(textBox_resumeFolderPath.Text, mStr.supportFileType);
            unrecordedResumeFilePathList = filePathList;
            Debug.Write("unrecordedResumeFilePathList.Count: " + unrecordedResumeFilePathList.Count);
            StringBuilder statement = new StringBuilder();
            statement.Append("SELECT filePath FROM " + mStr.dbRecordedFileTableName + " WHERE fileHash IN (");
            foreach (string filePath in filePathList)
            {
                string md5 = fileUtils.CalculateMD5(filePath);
                statement.Append("'" + md5 + "'");
                if (filePath != filePathList.Last())
                {
                    statement.Append(",");
                }
                else
                {
                    statement.Append(");");
                }
            }
            Debug.Write("statement.ToString(): " + statement.ToString());
            MySqlCommand cmd = new MySqlCommand(statement.ToString(), conn);
            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                int resultSet = 0;
                do
                {
                    Debug.Write("Result Set: {0}", (++resultSet).ToString());
                    while (dr.Read())
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            string path = (string)dr[i];
                            Debug.Write("path: " + path);
                            if (unrecordedResumeFilePathList.Contains(path))
                            {
                                unrecordedResumeFilePathList.Remove(path);
                            }
                        }
                    }
                } while (dr.NextResult());
            }
            Debug.Write("unrecordedResumeFilePathList.Count: " + unrecordedResumeFilePathList.Count);
            textBlock_unrecordedResumeFileNum.Text = mStr.textBlock_unrecordedResumeFileNum + unrecordedResumeFilePathList.Count.ToString();
        }

        private void buttonInsideListView_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            var resumeObj = currentResumeObj;
            var properties = resumeObj.GetType().GetProperties();
            Debug.Write("properties.Length: " + properties.Length);
            var count = 1;
            if ((string)btn.Tag == mStr.buttonInsideListViewErrorHandle)
            {

                foreach (System.Reflection.PropertyInfo prop in resumeObj.GetType().GetProperties())
                {
                    Debug.Write("count: " + count);
                    var value = prop.GetValue(resumeObj);
                    if (properties.Length == count)
                    {
                        errorResumeList.Remove(resumeObj);
                        mySqlUtils.insert(resumeObj, mStr.dbResumeTableName);
                        mySqlUtils.deleteFromErrorTable(resumeObj);
                    }
                    else if (value != null && value.ToString().Trim().Length != 0)
                    {
                        count++;
                    }
                }
            }
            else if ((string)btn.Tag == mStr.buttonInsideListViewQueryResult)
            {
                Debug.Write("\nresumeObj.fileHash: " + resumeObj.necessaryProp.fileHash);
                mySqlUtils.UpdateResumeTable(resumeObj);
            }
        }

        private void button_query_Click(object sender, RoutedEventArgs e)
        {
            var statement = new StringBuilder();
            statement.Append("SELECT * FROM " + mStr.dbResumeTableName + " WHERE ");
            //iterate through the child controls of "grid"
            var dict = new Dictionary<string, string>();
            int count = VisualTreeHelper.GetChildrenCount(grid_queryParameter);
            for (int i = 0; i < count; i++)
            {
                Debug.Write("\ncount: " + count);
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(grid_queryParameter, i);
                if (childVisual is System.Windows.Controls.CheckBox)
                {
                    var cb = (System.Windows.Controls.CheckBox)childVisual;
                    Debug.Write("\ncb.Content: " + cb.Content);
                    if (cb.IsChecked == true)
                    {
                        for (int n = 0; n < count; n++)
                        {
                            childVisual = (Visual)VisualTreeHelper.GetChild(grid_queryParameter, n);
                            if (childVisual is System.Windows.Controls.TextBox)
                            {
                                var tb = (System.Windows.Controls.TextBox)childVisual;
                                if (tb.Name.Contains((string)cb.Tag))
                                {
                                    Debug.Write("\ntb.Name: " + tb.Name);
                                    Debug.Write("\n(string)cb.Tag: " + (string)cb.Tag);
                                    dict.Add((string)cb.Content, tb.Text);
                                    break;
                                }

                            }
                        }
                    }
                }
            }
            Debug.Write("\ndict.Count: " + dict.Count);
            foreach (var item in dict)
            {
                if (mStr.queryParameterKeyNameWitchListValue.Contains(item.Key))
                {
                    Debug.Write("\nitem.Key: "+ item.Key);
                    statement.Append(" ( ");
                    string[] valueList = Regex.Split(item.Value, @"/|\s|,|\t|;");
                    foreach (var value in valueList)
                    {
                        if (value.Length>0)
                        {
                        statement.Append(value + " IN " + item.Key);
                        if (value != valueList.Last())
                        {
                            statement.Append(" OR ");
                        }
                        else
                        {
                            statement.Append(")");
                        }

                        }
                    }
                }
                else
                {
                    statement.Append(item.Key + " LIKE '%" + item.Value + "%'");
                }
                        if (item.Key != dict.Last().Key)
                        {
                            statement.Append(" AND ");
                        }
                    else
                    {
                        statement.Append(";");
                    }

                }
                Debug.Write("\nstatement.ToString(): " + statement.ToString());
                queryRersultList = mySqlUtils.selectFromTable(statement.ToString());
                listView_queryResult.ItemsSource = queryRersultList;
            }

        private void textBox_dbLogDate_TextChanged(object sender, RoutedEventArgs e)
        {
            if (textBox_dbLogDate.Text != "" && unrecordedResumeFilePathList.Count > 0)
            {
                groupBox_controlInvisible.IsEnabled = true;
            }
        }

        private void button_saveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            string fileName = currentResumeObj.necessaryProp.filePath.Split('\\').Last();
            // set a default file name
            savefile.FileName = fileName;
            // set filters - this can be done in properties as well
            savefile.Filter = "All files (*.*)|*.*";
            savefile.RestoreDirectory = true;

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(savefile.FileName, currentResumeObj.necessaryProp.fileBlob);
            }
        }

        private void textBox_queryParameter_skill_GotFocus(object sender, RoutedEventArgs e)
        {
            menu_skill.Visibility = Visibility.Visible;
        }

        private void textBox_queryParameter_skill_LostFocus(object sender, RoutedEventArgs e)
        {
            menu_skill.Visibility = Visibility.Hidden;
        }

        private void grid_skill_initial(int columnNum)
        {
            var rowNum = Math.Ceiling((float)mStr.skillDict.Count / columnNum);

            for (int col = 0; col < columnNum; col++)
            {
                grid_skill.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < rowNum; row++)
            {
                grid_skill.RowDefinitions.Add(new RowDefinition());
            }

            var c = 0;
            var r = 0;

            foreach (var item in mStr.skillDict)
            {
                var keyword = item.Value;
                System.Windows.Controls.CheckBox cb = new System.Windows.Controls.CheckBox();
                cb.Content = keyword;
                cb.Checked += checkBoxInsideMenu_skill_Checked;
                cb.Unchecked += checkBoxInsideMenu_skill_Unchecked;
                grid_skill.Children.Add(cb);
                Grid.SetColumn(cb, c);
                Grid.SetRow(cb, r);
                Debug.Write("c: " + c);
                Debug.Write("r: " + r);
                if (r < grid_skill.RowDefinitions.Count - 1)
                {
                    r++;
                }
                else if (r == grid_skill.RowDefinitions.Count - 1)
                {
                    r = 0;
                    c++;
                }
            }
            menu_skill.Height = grid_skill.Height;
            menu_skill.Width = grid_skill.Width;
            Debug.Write("\ngrid_skill.Children.Count: " + grid_skill.Children.Count);
            Debug.Write("\nmStr.resumeSkillKeywordList.Count: " + mStr.skillDict.Count);
        }

        private void checkBoxInsideMenu_skill_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.CheckBox;
            textBox_queryParameter_skill.Text += "/"+cb.Content;
        }

        private void checkBoxInsideMenu_skill_Unchecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.CheckBox;
            textBox_queryParameter_skill.Text = textBox_queryParameter_skill.Text.Replace("/" + cb.Content,"");
        }
    }
}