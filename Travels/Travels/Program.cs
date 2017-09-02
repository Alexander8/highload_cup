using System.Threading;
using Travels.Data.Dal;
using Travels.Data.Dal.Service;
using Travels.Data.Import;
using Travels.Server;

namespace Travels
{
    internal class Program
    {
        private static readonly Mutex Mutex = new Mutex();

        private static void Main()
        {
            ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads / 2, completionPortThreads / 2);

            InitServer();
            InitStorage();

            Mutex.WaitOne();
        }

        private static void InitServer()
        {
            SocketServer.Init();
        }

        private static void InitStorage()
        {
            var dataSource = new ArchiveDataSource();
            var data = dataSource.Read("/tmp/data/data.zip");

            Storage.LoadData(data);

            UpdateStorageService.Init();
        }
    }
}
