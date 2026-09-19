using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace CodexUsageTrayLite.Services
{
    internal sealed class OwnedProcessJob : IDisposable
    {
        private const uint JobObjectLimitKillOnJobClose = 0x00002000;
        private readonly SafeFileHandle handle;

        private OwnedProcessJob(SafeFileHandle handle)
        {
            this.handle = handle;
        }

        public static OwnedProcessJob TryCreate()
        {
            var rawHandle = CreateJobObject(IntPtr.Zero, null);
            if (rawHandle == IntPtr.Zero || rawHandle == new IntPtr(-1)) return null;
            var safeHandle = new SafeFileHandle(rawHandle, true);
            var information = new JobObjectExtendedLimitInformation();
            information.BasicLimitInformation.LimitFlags = JobObjectLimitKillOnJobClose;
            var length = Marshal.SizeOf(typeof(JobObjectExtendedLimitInformation));
            if (!SetInformationJobObject(safeHandle, 9, ref information, (uint)length))
            {
                safeHandle.Dispose();
                return null;
            }
            return new OwnedProcessJob(safeHandle);
        }

        public bool TryAssign(Process process)
        {
            if (process == null || handle == null || handle.IsInvalid || handle.IsClosed) return false;
            try
            {
                return AssignProcessToJobObject(handle, process.Handle);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public bool TryGetActiveProcessCount(out uint activeProcesses)
        {
            activeProcesses = 0;
            if (handle == null || handle.IsInvalid || handle.IsClosed) return false;
            JobObjectBasicAccountingInformation information;
            if (!QueryInformationJobObject(
                handle,
                1,
                out information,
                (uint)Marshal.SizeOf(typeof(JobObjectBasicAccountingInformation)),
                IntPtr.Zero))
            {
                return false;
            }
            activeProcesses = information.ActiveProcesses;
            return true;
        }

        public bool WaitForNoActiveProcesses(int graceMilliseconds, int pollMilliseconds, out uint activeProcesses, out long waitedMilliseconds)
        {
            if (graceMilliseconds < 0) throw new ArgumentOutOfRangeException("graceMilliseconds");
            if (pollMilliseconds <= 0) throw new ArgumentOutOfRangeException("pollMilliseconds");

            var watch = Stopwatch.StartNew();
            while (true)
            {
                if (!TryGetActiveProcessCount(out activeProcesses))
                {
                    watch.Stop();
                    waitedMilliseconds = watch.ElapsedMilliseconds;
                    return false;
                }
                if (activeProcesses == 0 || watch.ElapsedMilliseconds >= graceMilliseconds)
                {
                    watch.Stop();
                    waitedMilliseconds = watch.ElapsedMilliseconds;
                    return true;
                }
                var remaining = graceMilliseconds - (int)Math.Min(int.MaxValue, watch.ElapsedMilliseconds);
                Thread.Sleep(Math.Min(pollMilliseconds, Math.Max(1, remaining)));
            }
        }

        public void Dispose()
        {
            if (handle != null) handle.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IoCounters
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JobObjectBasicLimitInformation
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JobObjectExtendedLimitInformation
        {
            public JobObjectBasicLimitInformation BasicLimitInformation;
            public IoCounters IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JobObjectBasicAccountingInformation
        {
            public long TotalUserTime;
            public long TotalKernelTime;
            public long ThisPeriodTotalUserTime;
            public long ThisPeriodTotalKernelTime;
            public uint TotalPageFaultCount;
            public uint TotalProcesses;
            public uint ActiveProcesses;
            public uint TotalTerminatedProcesses;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr jobAttributes, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(
            SafeFileHandle job,
            int informationClass,
            ref JobObjectExtendedLimitInformation information,
            uint informationLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryInformationJobObject(
            SafeFileHandle job,
            int informationClass,
            out JobObjectBasicAccountingInformation information,
            uint informationLength,
            IntPtr returnLength);
    }
}
