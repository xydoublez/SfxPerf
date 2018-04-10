using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace SfxPerf
{
    class Program
    {
        static PerformanceCounter cpu = null;
        static PerformanceCounter memory = null;
        static PerformanceCounter cFree = null;
        static PerformanceCounter AvgDiskQueueLength = null;
        static PerformanceCounter AvgDiskSecRead = null;
        static PerformanceCounter AvgDiskSecWrite= null;
        static PerformanceCounter CurrentDiskQueueLength = null;
        static PerformanceCounter AvgDiskSecTransfer = null;
        static PerformanceCounter BufferCacheHitRatio = null;
        static PerformanceCounter PageLifeExpectancy = null;
        static PerformanceCounter BatchRequestsSec = null;
        static PerformanceCounter SQLCompilationsSec = null;
        static PerformanceCounter ReCompilationsSec = null;
        static PerformanceCounter UserConnections = null;
        static PerformanceCounter LockWaitsSec = null;
        static PerformanceCounter ProcessesBlocked = null;
        static PerformanceCounter CheckpointPagesSec = null;
        //IIS
        static PerformanceCounter iisCurrentConnections = null;
        static PerformanceCounter aspnetRequestsCurrent = null;
        static PerformanceCounter aspnetRequestsQueued = null;

        //network
        static PerformanceCounter tcpConnectionsEstablished = null;
        //第一个网卡
        static PerformanceCounter[] nics = null;
        //第二个网卡
        static PerformanceCounter[] nics2 = null;

        static void Main(string[] args)
        {

            Console.WriteLine("开始搜集性能数据.....................");
            init();
            StringBuilder result = new StringBuilder();
            while (true)
            {

                try
                {
                    result.Clear();
                    result.AppendLine("************************************************************");
                    result.AppendLine("时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    GetSystemInfo(result);
                    GetIIS(result);
                    GetSqlServer(result);
                    GetNetwork(result);
                    result.AppendLine("************************************************************");
                    Console.WriteLine(result.ToString());
                    File.AppendAllText("sfxperf.txt", result.ToString());
                    Thread.Sleep(1000);
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message+ex.StackTrace);
                    return;
                }
               
            }



        }
        static void init()
        {
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
                if (PerformanceCounterCategory.Exists("PhysicalDisk") && PerformanceCounterCategory.CounterExists("Avg. Disk Queue Length", "PhysicalDisk"))
                {
                    AvgDiskQueueLength = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total");
                }
                if (PerformanceCounterCategory.Exists("PhysicalDisk") && PerformanceCounterCategory.CounterExists("Avg. Disk sec/Read", "PhysicalDisk"))
                {
                    AvgDiskSecRead = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
                }
                if (PerformanceCounterCategory.Exists("PhysicalDisk") && PerformanceCounterCategory.CounterExists("Avg. Disk sec/Write", "PhysicalDisk"))
                {
                    AvgDiskSecWrite = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
                }
                if (PerformanceCounterCategory.Exists("PhysicalDisk") && PerformanceCounterCategory.CounterExists("Current Disk Queue Length", "PhysicalDisk"))
                {
                    CurrentDiskQueueLength = new PerformanceCounter("PhysicalDisk", "Current Disk Queue Length", "_Total");
                }
                if (PerformanceCounterCategory.Exists("PhysicalDisk") && PerformanceCounterCategory.CounterExists("Avg. Disk sec/Transfer", "PhysicalDisk"))
                {
                    AvgDiskSecTransfer = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Transfer", "_Total");
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
                    if (PerformanceCounterCategory.InstanceExists("_Total", "SQLServer:Locks"))
                    {
                        LockWaitsSec = new PerformanceCounter("SQLServer:Locks", "Lock Waits/sec", "_Total");
                    }

                }
                if (PerformanceCounterCategory.Exists("SQLServer:General Statistics") && PerformanceCounterCategory.CounterExists("Processes blocked", "SQLServer:General Statistics"))
                {
                    ProcessesBlocked = new PerformanceCounter("SQLServer:General Statistics", "Processes blocked");
                }
                if (PerformanceCounterCategory.Exists("SQLServer:Buffer Manager") && PerformanceCounterCategory.CounterExists("Checkpoint Pages/Sec", "SQLServer:Buffer Manager"))
                {
                    CheckpointPagesSec = new PerformanceCounter("SQLServer:Buffer Manager", "Checkpoint Pages/Sec");
                }
                #region IIS
                if (PerformanceCounterCategory.Exists("Web Service") && PerformanceCounterCategory.CounterExists("Current Connections", "Web Service"))
                {
                    iisCurrentConnections = new PerformanceCounter("Web Service", "Current Connections", "_Total");
                }
                if (PerformanceCounterCategory.Exists("ASP.NET") && PerformanceCounterCategory.CounterExists("Requests Current", "ASP.NET"))
                {
                    aspnetRequestsCurrent = new PerformanceCounter("ASP.NET", "Requests Current");
                }
                if (PerformanceCounterCategory.Exists("ASP.NET") && PerformanceCounterCategory.CounterExists("Requests Queued", "ASP.NET"))
                {
                    aspnetRequestsQueued = new PerformanceCounter("ASP.NET", "Requests Queued");
                }
                
                #endregion
                if (PerformanceCounterCategory.Exists("TCPv4") && PerformanceCounterCategory.CounterExists("Connections Established", "TCPv4"))
                {
                    tcpConnectionsEstablished = new PerformanceCounter("TCPv4", "Connections Established");
                }
                if (PerformanceCounterCategory.Exists("Network Interface"))
                {
                    List<string> nis = new List<string>();
                    NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    for (int i = 0; i < allNetworkInterfaces.Length; i++)
                    {
                        NetworkInterface networkInterface = allNetworkInterfaces[i];
                        if (networkInterface.OperationalStatus == OperationalStatus.Up
                            && (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                                ||networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                                )
                        {
                            nis.Add(networkInterface.Description);
                        }
                    }
                    PerformanceCounterCategory nicCategory = new PerformanceCounterCategory("Network Interface");
                    if (nicCategory != null)
                    {
                        if (nis.Count > 0)
                        {
                            var ins = nis[0].Replace("(", "[").Replace(")", "]").Replace("/", "_");
                            if (PerformanceCounterCategory.InstanceExists(ins, "Network Interface"))
                            {
                                nics = nicCategory.GetCounters(ins);
                            }
                        }
                    }
                    if (nis.Count > 1)
                    {
                        var ins = nis[0].Replace("(", "[").Replace(")", "]").Replace("/", "_");
                        if (PerformanceCounterCategory.InstanceExists(ins, "Network Interface"))
                        {
                            nics2 = nicCategory.GetCounters(ins);
                        }
                    }
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine("init失败"+ex.Message + ex.StackTrace);
            }
        }
        static void GetSystemInfo(StringBuilder result)
        {
            result.AppendLine("=====系统基本信息======");
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
            if(AvgDiskQueueLength != null)
            {
                result.AppendLine("AvgDiskQueueLength: " + AvgDiskQueueLength.NextValue()+ ",理想值队列长度	小于 2");
            }
            if(CurrentDiskQueueLength != null)
            {
                result.AppendLine("CurrentDiskQueueLength: " + CurrentDiskQueueLength.NextValue() + ",理想值队列长度	小于 2");
            }
            if (AvgDiskSecTransfer != null)
            {
                result.AppendLine("AvgDiskSecTransfer: " + AvgDiskSecTransfer.NextValue());
            }
            if (AvgDiskSecRead != null)
            {
                result.AppendLine("AvgDiskSecRead: " + AvgDiskSecRead.NextValue()+ ",一般不要超过11～15ms");
            }
            if (AvgDiskSecWrite != null)
            {
                result.AppendLine("AvgDiskSecWrite: " + AvgDiskSecWrite.NextValue() + ",一般建议小于12ms");
            }
        }
        static void GetSqlServer(StringBuilder result)
        {
            if(LockWaitsSec == null)
            {
                result.AppendLine("====无数据库服务====");
                return;
            }
            result.AppendLine("=======数据库=========");
            
            if (BufferCacheHitRatio != null)
            {
                result.AppendLine("BufferCacheHitRatio: " + BufferCacheHitRatio.NextValue() + "%");
            }
            if (PageLifeExpectancy != null)
            {
                result.AppendLine(string.Format("PageLifeExpectancy:{0}，低于300（或5分钟），说明我们的服务器内存不足 ", PageLifeExpectancy.NextValue()));
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
                result.AppendLine("LockWaitsSec: " + LockWaitsSec.NextValue() + ",要保持这个计数器为零或接近零。");
            }
            if (ProcessesBlocked != null)
            {
                result.AppendLine("ProcessesBlocked: " + ProcessesBlocked.NextValue());
            }
            if (CheckpointPagesSec != null)
            {
                result.AppendLine("CheckpointPagesSec: " + CheckpointPagesSec.NextValue());
            }
        }
        static void GetIIS(StringBuilder result)
        {
            result.AppendLine("=========IIS===========");
            if (iisCurrentConnections != null)
            {
                result.AppendLine("iisCurrentConnections: " + iisCurrentConnections.NextValue());
            }
            if (aspnetRequestsCurrent != null)
            {
                result.AppendLine("aspnetRequestsCurrent: " + aspnetRequestsCurrent.NextValue());
            }
            if (aspnetRequestsQueued != null)
            {
                result.AppendLine("aspnetRequestsQueued: " + aspnetRequestsQueued.NextValue());
            }

        }
        static void GetNetwork(StringBuilder result)
        {
         
            
            result.AppendLine("=====获取网络数据=====");
            if (tcpConnectionsEstablished != null)
            {
                result.AppendLine("tcpConnectionsEstablished TCP建立的连接数: " + tcpConnectionsEstablished.NextValue());
            }
            //if (currentBideWith != null)
            //{
            //    result.AppendLine("currentBideWith 当前带宽: " + currentBideWith.NextValue());
            //}
            float total = 0;
            float bideWith = 0;
            if (nics != null)
            {
               
                foreach (var c in nics)
                {
                    //result.AppendLine(c.CounterHelp);
                    if (c.CounterName == "Bytes Total/sec")
                    {
                        total = c.NextValue();
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + total.ToString("#"));

                    }
                    else if (c.CounterName == "Current Bandwidth")
                    {
                        bideWith = c.NextValue();
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + bideWith.ToString("#"));
                    }
                    else
                    {
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + c.NextValue().ToString("#"));
                    }
                }
                result.AppendLine("网络使用率%:" + ((total*8)/bideWith)*100);
            }
            if (nics2 != null)
            {
                foreach (var c in nics2)
                {
                    if (c.CounterName == "Bytes Total/sec")
                    {
                        total = c.NextValue();
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + total.ToString("#"));

                    }
                    else if (c.CounterName == "Current Bandwidth")
                    {
                        bideWith = c.NextValue();
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + bideWith.ToString("#"));
                    }
                    else
                    {
                        result.AppendLine(c.CounterName + "(" + c.InstanceName + "):" + c.NextValue().ToString("#"));
                    }
                }
                result.AppendLine("网络使用率%:" + ((total * 8) / bideWith)*100);
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
