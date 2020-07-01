using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ResumeDatabase.lib
{
    static class infoExtract
    {
        public static resumeObj get(string content, Dictionary<string, string> skillDict)
        {
            resumeObj ensumeObj = new resumeObj();
            generalPropExtract(content, ensumeObj.necessaryProp);
            optionPropExtract(content, ensumeObj.optionProp);
            skillExtract(content, ensumeObj, skillDict);
            return ensumeObj;
        }

        public static void generalPropExtract(string content, necessaryProp necessaryProp)
        {
            var regexObjList = new List<regexObj>()
            {
                new regexObj("name", @"姓\s*名[:：\s]*([\u4e00-\u9fa5]{2,4})", 1),
                //new regexObj("name", @"^[^\u4e00-\u9fa5]*([\u4e00-\u9fa5]{2,4})", 1),
                new regexObj("mail", @"(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}))", 1),
                new regexObj("phone", @"((\d{11})|(\d{3}(-|－)\d{4}(-|－)\d{4}))", 1),
                new regexObj("gender", @"(性\s*别[:：\s]*)?((男|女){1})", 2),
                new regexObj("brithday", @"(出生年月|出生日期)[:：\s]*(\d{4}(\.|-|年)\d{1,2}((\.|-|月)\d{1,2})?日?)", 2),
                new regexObj("brithday", @"年\s*龄[:：\s]*(\d{2})", 1),
                new regexObj("brithday", @"(\d{2})\s*岁", 1),
                new regexObj("region", @"(所在地|现居住地|现居住|(现\s*居)|所在地区)[:：\s]{1,9}([\u4e00-\u9fa5]*)", 3),
            };
            foreach(regexObj regexObj in regexObjList)
            {
                    Match matchResult = Regex.Match(content, regexObj.regex,RegexOptions.Singleline);
                    if (matchResult.Success)
                    {
                        Debug.Write("\nmatchResult: " + matchResult.Groups[regexObj.backReference].Value);
                        typeof(necessaryProp).GetProperty(regexObj.field).SetValue(necessaryProp, matchResult.Groups[regexObj.backReference].Value);
                    }
                    else
                    {
                        Debug.Write("\nmatchResult(failed): " + regexObj.field + "----" + regexObj.regex);
                    }
            }
        }

        public static void optionPropExtract(string content, optionProp optionProp)
        {
            var regexObjList = new List<regexObj>()
            {
                new regexObj("japanese", @"((N[123])|(日语.{1}级))", 1),
                new regexObj("intentionSalary", @"((期望薪资|期望月薪|期望年薪)[:：\s]*([\w-\d\u4e00-\u9fa5]*)|(\d{1,2}-\d{1,2}K))", 1),
                new regexObj("intentionRegion", @"(期望地点|期望工作地区|地点)[:：\s]*([\u4e00-\u9fa5]*)", 2),
            };
            //Debug.Write("\ncontent: " + content);
            content = Regex.Replace(content,@"\r\n","aaa");
            //content = content.Replace(System.Environment.NewLine, ");
            Debug.Write("\ncontent: " + content);
            foreach (regexObj regexObj in regexObjList)
            {
                    Match matchResult = Regex.Match(content, regexObj.regex, RegexOptions.Singleline);
                    if (matchResult.Success)
                    {
                        Debug.Write("\nmatchResult: " + matchResult.Groups[regexObj.backReference].Value);
                        typeof(optionProp).GetProperty(regexObj.field).SetValue(optionProp, matchResult.Groups[regexObj.backReference].Value);
                    }
                    else
                    {
                        Debug.Write("\nmatchResult(failed): " + regexObj.field + "----" + regexObj.regex);
                    }
            }
        }

        public static void skillExtract(string content, resumeObj ensumeObj,Dictionary<string,string> skillDict)
        {
            var skill = new StringBuilder();
            foreach(var item in skillDict)
            {
                if (content.ToLower().Contains(item.Value))
                {
                    skill.Append(item.Value);
                    skill.Append("/");
                }
            }
            if(skill.Length>0)
            {
                ensumeObj.necessaryProp.skill = skill.ToString();
            }
        }
    }

    class regexObj
    {
        public string field { set; get; }
        public string regex { set; get; }
        public int backReference { set; get; }

        public regexObj(string field, string regex, int backReference)
        {
            this.field = field;
            this.regex = regex;
            this.backReference = backReference;
        }
    }
}
