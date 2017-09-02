using Newtonsoft.Json;
using System;
using System.Threading;
using Travels.Data.Dto;

namespace Travels.Data.Dal.Service
{
    internal static class UpdateStorageService
    {
        private static readonly Thread UpdateStorageThread;
        private static readonly AutoResetEvent NewRequestEvent = new AutoResetEvent(false);
        private static volatile bool _eventFired = false;
        private static readonly UpdateStorageOperationDto[] Queue = new UpdateStorageOperationDto[50000];
        private static volatile int _operationIndex = -1;

        static UpdateStorageService()
        {
            UpdateStorageThread = new Thread(UpdateStorage)
            {
                Name = "UpdateStorageThread"
            };
        }

        public static void Init()
        {
            UpdateStorageThread.Start();
            Console.WriteLine("Update storage service initialized");
        }

        public static void EnqueueCreateUser(CreateUserParamsDto createUserParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.CreateUser, createUserParams);
        }

        public static void EnqueueUpdateUser(UpdateUserParamsDto updateUserParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.UpdateUser, updateUserParams);
        }

        public static void EnqueueCreateLocation(CreateLocationParamsDto createLocationParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.CreateLocation, createLocationParams);
        }

        public static void EnqueueUpdateLocation(UpdateLocationParamsDto updateLocationParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.UpdateLocation, updateLocationParams);
        }

        public static void EnqueueCreateVisit(CreateVisitParamsDto createVisitParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.CreateVisit, createVisitParams);
        }

        public static void EnqueueCreateVisit(UpdateVisitParamsDto updateVisitParams)
        {
            EnqueueUpdateOperation(UpdateStorageOperationType.UpdateVisit, updateVisitParams);
        }

        private static void EnqueueUpdateOperation(UpdateStorageOperationType type, object updateParams)
        {
            var newVal = Interlocked.Increment(ref _operationIndex);

            Queue[newVal] = new UpdateStorageOperationDto
            {
                Type = type,
                Params = updateParams
            };

            if (!_eventFired)
            {
                Console.WriteLine($"[{DateTime.Now}] Woke up UpdateStorage thread");
                _eventFired = true;
                NewRequestEvent.Set();
            }
        }

        private static void UpdateStorage()
        {
            NewRequestEvent.WaitOne();
            var processedOperationIndex = 0;

            do
            {
                while (processedOperationIndex <= _operationIndex)
                {
                    UpdateStorageOperationDto operation = null;

                    try
                    {
                        operation = Queue[processedOperationIndex];

                        switch (operation.Type)
                        {
                            case UpdateStorageOperationType.CreateUser:
                                Storage.CreateUser((CreateUserParamsDto)operation.Params);
                                break;
                            case UpdateStorageOperationType.UpdateUser:
                                Storage.UpdateUser((UpdateUserParamsDto)operation.Params);
                                break;
                            case UpdateStorageOperationType.CreateLocation:
                                Storage.CreateLocation((CreateLocationParamsDto)operation.Params);
                                break;
                            case UpdateStorageOperationType.UpdateLocation:
                                Storage.UpdateLocation((UpdateLocationParamsDto)operation.Params);
                                break;
                            case UpdateStorageOperationType.CreateVisit:
                                Storage.CreateVisit((CreateVisitParamsDto)operation.Params);
                                break;
                            case UpdateStorageOperationType.UpdateVisit:
                                Storage.UpdateVisit((UpdateVisitParamsDto)operation.Params);
                                break;
                        }

                        ++processedOperationIndex;
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Type: {operation?.Type}, Params: '{(operation == null || operation.Params == null ? "null" : JsonConvert.SerializeObject(operation.Params))}', Ex: {ex}";
                        Console.WriteLine(msg);
                    }

                    if (processedOperationIndex % 1000 == 0)
                        Console.WriteLine($"[{DateTime.Now}] Processed: {processedOperationIndex}, Total: {_operationIndex}");
                }

                Thread.Sleep(10);
            }
            while (true);
        }
    }
}
