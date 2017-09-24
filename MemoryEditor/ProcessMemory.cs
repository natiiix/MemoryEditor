using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MemoryEditor
{
    public static class ProcessMemory
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        public enum AllocationProtectEnum : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        public enum StateEnum : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        public enum TypeEnum : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public AllocationProtectEnum AllocationProtect;
            public IntPtr RegionSize;
            public StateEnum State;
            public AllocationProtectEnum Protect;
            public TypeEnum Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            private ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        public static int Read(int processId, IntPtr address)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.VMRead, false, processId);

            byte[] buffer = new byte[sizeof(int)];

            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out IntPtr bytesRead);

            return BitConverter.ToInt32(buffer, 0);
        }

        public static bool Write(int processId, IntPtr address, int value)
        {
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, processId);

            byte[] buffer = BitConverter.GetBytes(value);

            return WriteProcessMemory(processHandle, address, buffer, buffer.Length, out IntPtr bytesWritten);
        }

        public static List<IntPtr> Find(int processId, int targetValue)
        {
            List<IntPtr> pointers = new List<IntPtr>();

            // getting minimum & maximum address

            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            //IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            //IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            UInt64 min_addr = (UInt64)sys_info.minimumApplicationAddress;
            UInt64 max_addr = (UInt64)sys_info.maximumApplicationAddress;
            //UInt64 max_addr = 0xFFFFFFFF;

            // opening the process with desired access level
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead, false, processId);

            // this will store any information we get from VirtualQueryEx()
            MEMORY_BASIC_INFORMATION mem_basic_info = new MEMORY_BASIC_INFORMATION();

            while (min_addr < max_addr)
            {
                // 28 = sizeof(MEMORY_BASIC_INFORMATION)
                //System.Windows.MessageBox.Show(VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, (uint)Marshal.SizeOf(mem_basic_info)).ToString());
                VirtualQueryEx(processHandle, (IntPtr)min_addr, out mem_basic_info, (uint)Marshal.SizeOf(mem_basic_info));

                // if this memory chunk is accessible
                if (mem_basic_info.Protect == AllocationProtectEnum.PAGE_READWRITE && mem_basic_info.State == StateEnum.MEM_COMMIT)
                {
                    byte[] buffer = new byte[(int)mem_basic_info.RegionSize];

                    // read everything in the buffer above
                    ReadProcessMemory(processHandle, mem_basic_info.BaseAddress, buffer, (int)mem_basic_info.RegionSize, out IntPtr bytesRead);

                    // Get the size of the target data type
                    int targetTypeSize = sizeof(int);

                    // then output this in the file
                    for (int i = 0; i < (int)mem_basic_info.RegionSize - (targetTypeSize - 1); i += targetTypeSize)
                    {
                        //Console.WriteLine("0x{0} : {1}", (mem_basic_info.BaseAddress + i).ToString("X"), (char)buffer[i]);
                        if (BitConverter.ToInt32(buffer, i) == targetValue)
                        {
                            pointers.Add(mem_basic_info.BaseAddress + i);
                        }
                    }
                }

                // move to the next memory chunk
                min_addr += (UInt64)mem_basic_info.RegionSize;
            }

            return pointers;
        }
    }
}