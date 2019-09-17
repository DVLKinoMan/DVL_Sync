using DVL_Sync.Abstractions;
using DVL_Sync.Extensions;
using DVL_Sync_FileEventsLogger.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Collections.Generic;
using System.Extensions;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DVL_Sync.Implementations
{
    public class FoldersSyncer : IFoldersSyncer
    {
        private IFolderOperationEventsReader _folderOperationEventsReader;

        public FoldersSyncer(IFolderOperationEventsReader folderOperationEventsReader)
        {
            _folderOperationEventsReader = folderOperationEventsReader;
        }

        public void SyncFolders(FolderConfig folderConfig1, FolderConfig folderConfig2)
        {
            var folderConfig1List = GetDateTimeAndOperationEventsFromFolderConfig(folderConfig1).SelectMany(t => t.OperationEvents).FilteredOperationEvents().ToList();
            var folderConfig2List = GetDateTimeAndOperationEventsFromFolderConfig(folderConfig2).SelectMany(t => t.OperationEvents).FilteredOperationEvents().ToList();

            //Remove OperationEvents from both Lists
            FilterOperationEvents(folderConfig1List, folderConfig1.FolderPath, folderConfig2List, folderConfig2.FolderPath);

            var operationFactory = new OperationFactoryViaOperationEvent(folderConfig1.FolderPath);
            var ops = folderConfig1List.GetOperations(operationFactory).FilterOperations().ToList();
            ops.ExecuteAll(folderConfig2.FolderPath);

            var operationFactory2 = new OperationFactoryViaOperationEvent(folderConfig2.FolderPath);
            folderConfig2List.GetOperations(operationFactory2).FilterOperations().ExecuteAll(folderConfig2.FolderPath);
        }

        /// <summary>
        /// Dummy Async method ???todo
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="folderConfig1"></param>
        /// <param name="folderConfig2"></param>
        /// <returns></returns>
        public async Task SyncFoldersAsync(CancellationToken cancellationToken, FolderConfig folderConfig1, FolderConfig folderConfig2)
        {
            SyncFolders(folderConfig1, folderConfig2);
            await Task.CompletedTask;
        }
   
        /// <summary>
        /// Remove unnecessary OperationEvents from both Lists
        /// </summary>
        /// <param name="operationEvents1"></param>
        /// <param name="folderPath1"></param>
        /// <param name="operationEvents2"></param>
        /// <param name="folderPath2"></param>
        private static void FilterOperationEvents(List<OperationEvent> operationEvents1, string folderPath1, List<OperationEvent> operationEvents2, string folderPath2)
        {
            var dic = new Dictionary<string, List<OperationEvent>>();

            void AddToDic(string folderPath, OperationEvent ev, bool inFirstList)
            {
                string filePath = ev is RenameOperationEvent renOp ? renOp.OldFilePath.SubtractPath(folderPath) : ev.FilePath.SubtractPath(folderPath);

                if (dic.ContainsKey(filePath))
                    dic[filePath].Add(ev);
                else dic.Add(filePath, new List<OperationEvent> { ev });
            }

            foreach(var ev in operationEvents1)
                AddToDic(folderPath1, ev, true);

            foreach (var ev in operationEvents2)
                AddToDic(folderPath2, ev, false);

            var neededOpEvents = new List<OperationEvent>();
            foreach(var pair in dic)
                neededOpEvents.AddRange(pair.Value.FilteredOperationEvents());
            //neededOpEvents.Add(pair.Value.OrderBy(t => t.RaisedTime).Last());

            operationEvents1.RemoveAll(op => !neededOpEvents.Contains(op));
            operationEvents2.RemoveAll(op => !neededOpEvents.Contains(op));
        }

        private IEnumerable<(DateTime DateTime, IEnumerable<OperationEvent> OperationEvents)> GetDateTimeAndOperationEventsFromFolderConfig(FolderConfig folderConfig)
        {
            foreach(var filePath in Directory.GetFiles(folderConfig.FolderPath))
                if (folderConfig.IsLogFileWithLogName(filePath, Constants.JsonLogFileName))
                {
                    string fileName = Path.GetFileName(filePath);
                    var dateTime = fileName.GetCustomDateTime();
                    yield return (dateTime, _folderOperationEventsReader.ReadOperationEvents(filePath));
                }
        }
 }
}
