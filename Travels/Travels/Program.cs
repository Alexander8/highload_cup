using System;
using System.Runtime;
using System.Threading;
using Travels.Data.Dal;
using Travels.Data.Import;
using Travels.Data.Util;
using Travels.Server;

namespace Travels
{
    internal class Program
    {
        private static readonly Mutex Mutex = new Mutex();

        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
            ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads / 2, completionPortThreads / 2);

            InitStorage();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            Console.WriteLine("IsServerGC: " + GCSettings.IsServerGC);

            InitServer();

            Mutex.WaitOne();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled exception. IsTerminating: {e.IsTerminating}, {e.ExceptionObject}");
        }

        private static void InitStorage()
        {
            var dataSource = new ArchiveDataSource();
            var data = dataSource.Read("/tmp/data/data.zip", "/tmp/data/options.txt");

            DatetimeUtil.CurrentTimestamp = data.CurrentTimestamp;
            Console.WriteLine($"CurrentTimestamp: {DatetimeUtil.CurrentTimestamp}");

            Storage.LoadData(data);
        }

        private static void InitServer()
        {
            SocketServer.Init();
        }
    }
}
