using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Reflection;

namespace ResumeDatabase.lib
{
    public class mySqlUtils
    {
        MainWindow parent;

        public mySqlUtils(MainWindow mainWindow)
        {
            parent = mainWindow;
        }

        public void ensureTableExist(string tableName)
        {
            string statement = "SELECT 1 FROM " + tableName + " LIMIT 1;";
            var cmd = new MySqlCommand(statement, parent.conn);
            bool exist = false;
            try
            {
                exist = (int)cmd.ExecuteScalar() == 1;
            }
            catch (Exception e) { }
            if (!exist)
            {
                if (tableName == parent.mStr.dbRecordedFileTableName)
                {
                    statement = "CREATE TABLE IF NOT EXISTS " + tableName + " (logtime DATETIME DEFAULT CURRENT_TIMESTAMP, fileHash char(40) UNIQUE, filePath tinytext);";
                }
                else if (tableName == parent.mStr.dbResumeTableName || tableName == parent.mStr.dbErrorResumeTableName)
                //else if (Regex.Match(tableName, @"\d{4}_\d{1,2}").Success || tableName == parent.mStr.dbErrorResumeTableName)
                {
                    statement = "CREATE TABLE IF NOT EXISTS " + tableName + " (logtime DATETIME DEFAULT CURRENT_TIMESTAMP, skill tinytext, logDate tinytext, fileHash char(40) UNIQUE, brithday tinytext, name tinytext, gender tinytext, region tinytext, " + parent.mStr.dbCreateTableSkillColumn + " phone tinytext, mail tinytext, fileBlob MediumBlob, filePath tinytext, overseaJob tinytext, japanese tinytext, intentionSalary tinytext, intentionRegion tinytext);";
                }
            }
            Debug.Write("ensureTableExist: " + statement);
            cmd = new MySqlCommand(statement, parent.conn);
            cmd.ExecuteScalar();
        }

        public void insert(resumeObj resumeObj, String tableName)
        {
            string statement = "INSERT IGNORE INTO " + tableName + " (logDate, name, brithday, gender, region, skill, " + parent.mStr.dbInsertSkillColumn + "  phone, mail, japanese, overseaJob, intentionSalary, intentionRegion, filePath, fileHash, fileBlob) VALUES ( @logDate, @name, @brithday, @gender, @region, @skill," + parent.mStr.dbInsertSkillToken + "@phone, @mail, @japanese, @overseaJob, @intentionSalary, @intentionRegion, @filePath, @fileHash, @fileBlob);";
            Debug.Write("insertToErrorTable: " + statement);
            MySqlCommand cmd = new MySqlCommand(statement, parent.conn);
            var objList = new object[2];
            objList[0] = resumeObj.necessaryProp;
            objList[1] = resumeObj.optionProp;
            foreach (object obj in objList)
            {
                var properties = obj.GetType().GetProperties();
                Debug.Write("properties.Length: " + properties.Length);
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    var name = prop.Name;
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        if(name== nameof(resumeObj.necessaryProp.fileBlob))
                        {
                            cmd.Parameters.Add("@"+name, MySqlDbType.Blob).Value = (byte[])value;
                        }
                        else if (name == nameof(resumeObj.necessaryProp.skill))
                        {
                            Debug.Write("\nname: " + name);
                            Debug.Write("\n(string)value: " + (string)value);
                            cmd.Parameters.Add("@" + name, MySqlDbType.String).Value = (string)value;
                            string[] skillList = Regex.Split(( string) value, @"/|\s|,|\t|;");
                            foreach (var skill in skillList)
                            {
                                if (skill.Length > 0)
                                {
                                    Debug.Write("\nskill: " + skill);
                                    cmd.Parameters.Add("@" + skill, MySqlDbType.Bit).Value = 1;
                                }
                            }
                            Debug.Write("\nhere");

                        }
                        else
                        {
                            Debug.Write("\nname: " + name);
                            Debug.Write("\n(string)value: " + (string)value);
                            cmd.Parameters.Add("@"+name, MySqlDbType.String).Value = (string)value;
                        }
                        Debug.Write("\nresumeObj.name: " + prop.Name);
                    }
                }
            }
            cmd.ExecuteNonQuery();
        }

        public void insertToRecordedFileTable(resumeObj resumeObj)
        {
            resumeObj.necessaryProp.filePath = resumeObj.necessaryProp.filePath.Replace("\\", "\\\\");
            string statement = string.Format("INSERT IGNORE INTO " + parent.mStr.dbRecordedFileTableName + " (filePath, fileHash) VALUES ('{0}','{1}')", resumeObj.necessaryProp.filePath, resumeObj.necessaryProp.fileHash);
            Debug.Write("insertToRecordedFileTable: " + statement);
            MySqlCommand cmd = new MySqlCommand(statement, parent.conn);
            cmd.ExecuteNonQuery();
        }

        public ObservableCollection<resumeObj> selectFromTable(string statement)
        {
            ObservableCollection<resumeObj> result = new ObservableCollection<resumeObj>();
            MySqlCommand cmd = new MySqlCommand(statement, parent.conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resumeObj resumeObj = new resumeObj();
                foreach (PropertyInfo secondaryProp in resumeObj.GetType().GetProperties())
                {
                    var secondaryObj = secondaryProp.GetValue(resumeObj);
                    foreach (PropertyInfo thridProp in secondaryObj.GetType().GetProperties())
                    {
                        var key = thridProp.Name;
                        Debug.Write("\nkey: " + key);
                        var value = reader[key];
                        Debug.Write("\nvalue: " + value);
                        if (value.GetType() != typeof(DBNull))
                        {
                            thridProp.SetValue(secondaryObj, value);
                        }
                    }
                    secondaryProp.SetValue(resumeObj, secondaryObj);
                }
                result.Add(resumeObj);
            }
            reader.Close();
            return result;
        }

        public void deleteFromErrorTable(resumeObj resumeObj)
        {
            string statement = string.Format("DELETE FROM " + parent.mStr.dbErrorResumeTableName + " WHERE fileHash='{0}'",resumeObj.necessaryProp.fileHash);
            MySqlCommand cmd = new MySqlCommand(statement, parent.conn);
            cmd.ExecuteNonQuery();
        }

        public void UpdateResumeTable(resumeObj resumeObj)
        {
            var statement = new StringBuilder();
            statement.Append("UPDATE " + parent.mStr.dbResumeTableName+" SET ");
            var objList = new object[2];
            objList[0] = resumeObj.necessaryProp;
            objList[1] = resumeObj.optionProp;
            foreach (object obj in objList)
            {
                var properties = obj.GetType().GetProperties();
                Debug.Write("properties.Length: " + properties.Length);
                var count = 1;
            foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
            {
                var name = prop.Name;
                var value = (string)prop.GetValue(obj);
                    if (name == nameof(resumeObj.necessaryProp.skill))
                    {
                        string[] skillList = Regex.Split(value, @"/|\s|,|\t|;");
                        foreach (var skill in skillList)
                        {
                            if (skill.Length > 0)
                            {
                                statement.Append(skill + "=1");
                                statement.Append(" OR ");
                            }
                            else if(skill==skillList.Last())
                            {
                                statement.Append(";");
                            }
                        }
                    }
                statement.Append(name + "='" + value + "'");
                if (properties.Length != count || (obj != objList.Last()&& properties.Length == count))
                {
                    statement.Append(", ");
                }
                count++;
            }
            }
            statement.Append(" WHERE fileHash='" + resumeObj.necessaryProp.fileHash + "';");
            Debug.Write("\nstatement.ToString(): "+ statement.ToString());
            MySqlCommand cmd = new MySqlCommand(statement.ToString(), parent.conn);
            cmd.ExecuteNonQuery();
        }
    }
}
