using System.Diagnostics;

namespace MemoryEditor
{
    public partial class ProcessInfo
    {
        public int Id { get; }
        public string ProcessName { get; }
        public string MainWindowTitle { get; }

        public ProcessInfo(Process proc)
        {
            Id = proc.Id;
            ProcessName = proc.ProcessName;
            MainWindowTitle = proc.MainWindowTitle;
        }

        public bool FitsFilter(string filter)
        {
            return ProcessName.ContainsCaseInsensitive(filter) || MainWindowTitle.ContainsCaseInsensitive(filter);
        }
    }
}