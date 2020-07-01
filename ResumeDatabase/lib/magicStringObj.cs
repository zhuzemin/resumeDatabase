using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeDatabase.lib
{
    public class magicStringObj
    {
        public string dbConnSuc { set; get; }
        public string dbConnFailed { set; get; }
        public string dbRecordedFileTableName { set; get; }
        public string dbErrorResumeTableName { set; get; }
        public string dbResumeTableName { set; get; }
        public string dbDetectTableNameFailed { set; get; }
        public string dbCreateTableSkillColumn { set; get; }
        public string dbInsertSkillToken { set; get; }
        public string dbInsertSkillColumn { set; get; }
        public string button_controlPause_Pause { set; get; }
        public string button_controlPause_Resume { set; get; }
        public string textBlock_unrecordedResumeFileNum { set; get; }
        public string textBoxPlaceholder { set; get; }
        public List<string> resumeFilenameExcludeKeywordList { set; get; }
        public string bossZhipinCharacteristic { set; get; }
        public string[] supportFileType { set; get; }
        public string tempFolderName { set; get; }
        public string[] overseaJobKeywordList { set; get; }
        public string overseaJobState_intended { set; get; }
        public string buttonInsideListViewErrorHandle { set; get; }
        public string buttonInsideListViewQueryResult { set; get; }
        public string[] queryParameterKeyNameWitchListValue { set; get; }
        public Dictionary<string, string> skillDict { set; get; }

        public magicStringObj()
        {
            dbConnSuc = "connection suc";
            dbConnFailed = "connection failed";
            dbRecordedFileTableName = "recordedFile";
            dbErrorResumeTableName = "errorResume";
            dbResumeTableName = "resume";
            dbDetectTableNameFailed = "generate tableName failed, please manual input.";
            button_controlPause_Pause = "pause";
            button_controlPause_Resume = "resume";
            textBlock_unrecordedResumeFileNum = "unrecorded resume: ";
            skillDict= new Dictionary<string, string>();
            skillDict.Add("_sql", "sql");
            skillDict.Add("shell", "shell");
            skillDict.Add("vba", "vba");
            skillDict.Add("net", "net");
            skillDict.Add("cobol", "cobol");
            skillDict.Add("abap", "abap");
            skillDict.Add("sap", "sap");
            skillDict.Add("android", "android");
            skillDict.Add("ios", "ios");
            skillDict.Add("cplusplus", "c++");
            skillDict.Add("angularjs", "angularjs");
            skillDict.Add("react", "react");
            skillDict.Add("java", "java");
            skillDict.Add("vue", "vue");
            skillDict.Add("python", "python");
            skillDict.Add("django", "django");
            skillDict.Add("nodejs", "nodejs");
            skillDict.Add("express", "express");
            skillDict.Add("drift", "drift");
            skillDict.Add("objectc", "object-c");
            skillDict.Add("csharp", "c#");
            skillDict.Add("_go", "go");
            textBoxPlaceholder = "e.g. 2020_4";
            resumeFilenameExcludeKeywordList =new List<string> { "智联招聘", "中文" ,"IOS","工程师", "开发", "赴日", "程序员", "经验", "简历", "猎聘"};
            overseaJobKeywordList = new string[] { "赴日","080","090","070" };
            overseaJobState_intended = "赴日";
            supportFileType = new string[]{"*.doc", "*.docx", "*.pdf"};
            buttonInsideListViewQueryResult = "queryResult";
            buttonInsideListViewErrorHandle = "errorHandle";
            queryParameterKeyNameWitchListValue = new string[] { "skill", "logDate" };
            dbCreateTableSkillColumn = dictToSql(" bit, ", skillDict);
            dbInsertSkillColumn = dictToSql(" , ", skillDict);
            dbInsertSkillToken = dictToSql("@", skillDict);
            tempFolderName = Directory.GetCurrentDirectory()+@"\temp";
            bossZhipinCharacteristic = @"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
+dgAAAAASUVORK5CYII=
";
        }

        public string dictToSql(string separator,Dictionary<string,string> dict)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var item in dict)
            {
                if (separator == "@")
                {
                    strBuilder.Append(separator);
                    strBuilder.Append(item.Key);
                    strBuilder.Append(", ");
                }
                else
                {
                strBuilder.Append(item.Key);
                strBuilder.Append(separator);

                }
            }
            return strBuilder.ToString();
        }
    }
}
