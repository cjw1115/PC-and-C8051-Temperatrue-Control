using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 上位机温度控制系统.Model
{
    public class CommandModel
    {
        public string SyncCode1 { get; set; }
        public string SyncCode2 { get; set; }
        public string SyncCode3 { get; set; }
        public string ControlCode { get; set; }

        public string ControlVal1 { get; set; }
        public string ControlVal2 { get; set; }

        public string Desc { get; set; }

        public void Clear()
        {
            SyncCode1 = null;
            SyncCode2 = null;
            SyncCode3 = null;
            ControlCode = null;
            ControlVal1 = null;
            ControlVal1 = null;
            Desc = null;
        }
    }
}
