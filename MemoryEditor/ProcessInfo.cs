using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MemoryEditor
{
    public class ProcessInfo
    {
        public readonly int Id;
        public readonly string ProcessName;

        public ProcessInfo(Process proc)
        {
            Id = proc.Id;
            ProcessName = proc.ProcessName;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, ProcessName);
        }
    }
}