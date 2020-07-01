using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeDatabase.lib
{
    public class resumeObj
    {
        public bool error { set; get; }
        public bool isEnabled { set; get; }
        public optionProp optionProp { set; get; }
        public necessaryProp necessaryProp { set; get; }


        public resumeObj()
        {
            error = false;
            isEnabled = false;
            optionProp = new optionProp();
            necessaryProp = new necessaryProp();
        }
    }

    public class necessaryProp
    {
        public string logDate { set; get; }
        public string name { set; get; }
        public string brithday { set; get; }
        public string gender { set; get; }
        public string skill { set; get; }
        public string region { set; get; }
        public string phone { set; get; }
        public string mail { set; get; }
        public string filePath { set; get; }
        public string fileHash { set; get; }
        public byte[] fileBlob { set; get; }

    }

    public class optionProp
    {
        public string intentionSalary { set; get; }
        public string intentionRegion { set; get; }
        public string japanese { set; get; }
        public string overseaJob { set; get; }
    }
}
