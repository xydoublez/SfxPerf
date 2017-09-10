using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SfxPerf
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetAll();
            //return;
            PerformanceCounter cpu = null;
            PerformanceCounter memory = null;
            PerformanceCounter cFree = null;
            PerformanceCounter BufferCacheHitRatio = null;
            PerformanceCounter PageLifeExpectancy = null;
            PerformanceCounter BatchRequestsSec = null;
            PerformanceCounter SQLCompilationsSec = null;
            PerformanceCounter ReCompilationsSec = null;
            PerformanceCounter UserConnections = null;
            PerformanceCounter LockWaitsSec = null;
            PerformanceCounter ProcessesBlocked = null;
            PerformanceCounter CheckpointPagesSec = null;

            try
            {
                if (PerformanceCounterCategory.Exists("Processor Information") && PerformanceCounterCategory.CounterExists("% Processor Time", "Processor Information"))
                {
                    cpu = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
                }
                if (PerformanceCounterCategory.Exists("Memory") && PerformanceCounterCategory.CounterExists("Available Mbytes", "Memory"))
                {
                    memory = new PerformanceCounter("Memory", "Available Mbytes");
                }
                if (PerformanceCounterCategory.Exists("LogicalDisk") && PerformanceCounterCategory.CounterExists("% Free Space", "LogicalDisk"))
                {
                    cFree = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:Buffer Manager") && PerformanceCounterCategory.CounterExists("Buffer cache hit ratio", "SQLServer:Buffer Manager"))
                {
                    BufferCacheHitRatio = new PerformanceCounter("SQLServer:Buffer Manager", "Buffer cache hit ratio");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:Buffer Manager") && PerformanceCounterCategory.CounterExists("Page life expectancy", "SQLServer:Buffer Manager"))
                {
                    PageLifeExpectancy = new PerformanceCounter("SQLServer:Buffer Manager", "Page life expectancy");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:SQL Statistics") && PerformanceCounterCategory.CounterExists("Batch Requests/sec", "SQLServer:SQL Statistics"))
                {
                    BatchRequestsSec = new PerformanceCounter("SQLServer:SQL Statistics", "Batch Requests/sec");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:SQL Statistics") && PerformanceCounterCategory.CounterExists("SQL Compilations/sec", "SQLServer:SQL Statistics"))
                {
                    SQLCompilationsSec = new PerformanceCounter("SQLServer:SQL Statistics", "SQL Compilations/sec");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:SQL Statistics") && PerformanceCounterCategory.CounterExists("Re-Compilations/Sec", "SQLServer:SQL Statistics"))
                {
                    ReCompilationsSec = new PerformanceCounter("SQLServer:SQL Statistics", "Re-Compilations/Sec");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:General Statistics") && PerformanceCounterCategory.CounterExists("User Connections", "SQLServer:General Statistics"))
                {
                    UserConnections = new PerformanceCounter("SQLServer:General Statistics", "User Connections");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:Locks") && PerformanceCounterCategory.CounterExists("Lock Waits/sec", "SQLServer:Locks"))
                {
                    LockWaitsSec = new PerformanceCounter("SQLServer:Locks", "Lock Waits/sec", "_Total");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:General Statistics") && PerformanceCounterCategory.CounterExists("Processes blocked", "SQLServer:General Statistics"))
                {
                    ProcessesBlocked = new PerformanceCounter("SQLServer:General Statistics", "Processes blocked");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:Buffer Manager") && PerformanceCounterCategory.CounterExists("Checkpoint Pages/Sec", "SQLServer:Buffer Manager"))
                {
                    CheckpointPagesSec = new PerformanceCounter("SQLServer:Buffer Manager", "Checkpoint Pages/Sec");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            StringBuilder result = new StringBuilder();
            while (true)
            {

                result.Clear();
                Thread.Sleep(1000);

                result.AppendLine("************************************************************");
                result.AppendLine("时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (cpu != null)
                {
                    result.AppendLine("CPU: " + cpu.NextValue() + "%");
                }
                if (memory != null)
                {
                    result.AppendLine("Memory: " + memory.NextValue() + "M");
                }
                if (cFree != null)
                {
                    result.AppendLine("C盘剩余空间率: " + cFree.NextValue() + "%");
                }
                result.AppendLine("01.数据库");
                if (BufferCacheHitRatio != null)
                {
                    result.AppendLine("BufferCacheHitRatio: " + BufferCacheHitRatio.NextValue() + "%");
                }
                if (PageLifeExpectancy != null)
                {
                    result.AppendLine("PageLifeExpectancy:{0}，低于300（或5分钟），说明我们的服务器内存不足 " + PageLifeExpectancy.NextValue());
                }
                if (BatchRequestsSec != null)
                {
                    result.AppendLine("BatchRequestsSec: " + BatchRequestsSec.NextValue());
                }

                if (SQLCompilationsSec != null)
                {
                    result.AppendLine("SQLCompilationsSec: " + SQLCompilationsSec.NextValue());
                }
                if (ReCompilationsSec != null)
                {
                    result.AppendLine("ReCompilationsSec: " + ReCompilationsSec.NextValue());
                }
                if (UserConnections != null)
                {
                    result.AppendLine("UserConnections: " + UserConnections.NextValue());
                }
                if (LockWaitsSec != null)
                {
                    result.AppendLine("LockWaitsSec: " + LockWaitsSec.NextValue());
                }
                if (ProcessesBlocked != null)
                {
                    result.AppendLine("ProcessesBlocked: " + ProcessesBlocked.NextValue());
                }
                if (CheckpointPagesSec != null)
                {
                    result.AppendLine("CheckpointPagesSec: " + CheckpointPagesSec.NextValue());
                }
                result.AppendLine("************************************************************");
                Console.WriteLine(result.ToString());
                File.AppendAllText("sfxperf.txt", result.ToString());
            }



        }

        public static string GetInstanceName(string categoryName, string counterName, Process p)
        {
            try
            {
                PerformanceCounterCategory processcounter = new PerformanceCounterCategory(categoryName);
                string[] instances = processcounter.GetInstanceNames();
                foreach (string instance in instances)
                {
                    PerformanceCounter counter = new PerformanceCounter(categoryName, counterName, instance);
                    //Logger.Info("对比in mothod GetInstanceName，" + counter.NextValue() + "：" + p.Id);
                    if (counter.NextValue() == p.Id)
                    {
                        return instance;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public static void GetCounterNameValueList(string CategoryName, string instanceName)
        {
            string[] instanceNames;
            ArrayList counters = new ArrayList();
            PerformanceCounterCategory mycat = new PerformanceCounterCategory(CategoryName);
            try
            {
                counters.AddRange(mycat.GetCounters(instanceName));
                foreach (PerformanceCounter counter in counters)
                {
                    Console.WriteLine(string.Format("{0,-30}:{1,20}", counter.CounterName, counter.RawValue));
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to list the counters for this category");
            }
        }
        static void GetCpu()
        {


        }
        static void GetAll()
        {


            PerformanceCounterCategory[] pcc = PerformanceCounterCategory.GetCategories();

            StringBuilder sb = new StringBuilder();
            string cn = null;
            for (int i = 0; i < pcc.Length; i++)
            {
                cn = pcc[i].CategoryName.ToUpper();
                //if (cn.IndexOf("PROCESSOR") != -1)
                {
                    sb.Remove(0, sb.Length);
                    sb.Append("CategoryName:" + pcc[i].CategoryName + "\r\n");
                    sb.Append("MachineName:" + pcc[i].MachineName + "\r\n");

                    string[] instanceNames = pcc[i].GetInstanceNames();
                    for (int j = 0; j < instanceNames.Length; j++)
                    {

                        sb.Append("**** Instance Name **********\r\n");
                        sb.Append("InstanceName:" + instanceNames[j] + "\r\n");
                        try
                        {
                            PerformanceCounter[] counters = pcc[i].GetCounters(instanceNames[j]);
                            for (int k = 0; k < counters.Length; k++)
                            {
                                sb.Append("CategoryName:" + counters[k].CategoryName + "\r\n");
                                sb.Append("CounterName:" + counters[k].CounterName + "\r\n");
                                sb.Append("value:" + counters[k].NextValue() + "\r\n");
                            }
                        }
                        catch (Exception)
                        { }
                        sb.Append("**************************************************\r\n");
                    }

                    Trace.TraceInformation(sb.ToString());
                    File.AppendAllText("counter.txt", sb.ToString());

                }

            }


        }

    }
}
