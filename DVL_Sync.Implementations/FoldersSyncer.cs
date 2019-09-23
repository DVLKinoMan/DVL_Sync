using DVL_Sync.Abstractions;
using DVL_Sync.Extensions;
//using DVL_Sync_FileEventsLogger.Implementations;
using DVL_Sync_FileEventsLogger.Models;
using Microsoft.Extensions.Logging;
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
        private IFoldersSyncReader _foldersSyncReader;
        private ILogger<FoldersSyncer> _logger;

        public FoldersSyncer(IFolderOperationEventsReader folderOperationEventsReader, IFoldersSyncReader foldersSyncReader, ILogger<FoldersSyncer> logger)
        {
            _folderOperationEventsReader = folderOperationEventsReader;
            _foldersSyncReader = foldersSyncReader;
            _logger = logger;
        }

        public void SyncFolders(FolderConfig folderConfig1, FolderConfig folderConfig2, string restorePointDirectoryPath)
        {
            var folderConfig1List = GetOperationEventsFromFolderConfig(folderConfig1).SelectMany(t => t.OperationEvents).FilteredOperationEvents().ToList();
            var folderConfig2List = GetOperationEventsFromFolderConfig(folderConfig2).SelectMany(t => t.OperationEvents).FilteredOperationEvents().ToList();

            //Remove OperationEvents from both Lists
            FilterOperationEvents(folderConfig1List, folderConfig1.FolderPath, folderConfig2List, folderConfig2.FolderPath);

            if (folderConfig1List.Count == 0 && folderConfig2List.Count == 0)
                return;

            string restorePointPath = CreateRestorePointDirectory(restorePointDirectoryPath);

            var operationFactory = new OperationFactoryViaOperationEvent(folderConfig1.FolderPath);
            folderConfig1List.GetOperations(operationFactory).FilterOperations().ExecuteAll(folderConfig2.FolderPath);

            var operationFactory2 = new OperationFactoryViaOperationEvent(folderConfig2.FolderPath);
            folderConfig2List.GetOperations(operationFactory2).FilterOperations().ExecuteAll(folderConfig2.FolderPath);

            _logger.LogDebug("Successfully synchronized folders: {folderPath1} and {folderPath2}", folderConfig1.FolderPath, folderConfig2.FolderPath);

            CreateRestorePoints(restorePointPath, folderConfig1, folderConfig2);
        }

        private string CreateRestorePointDirectory(string restorePointDirectoryPath)
        {
            int i = 1;
            string dirString = $"{restorePointDirectoryPath} {DateTime.Now.ToString("MM-dd-yyyy")}";
            while (Directory.Exists($"{dirString} ({i++})"))
                ;
            var dir = Directory.CreateDirectory($"{dirString} ({--i})");
            return dir.FullName;
        }

        private void CreateRestorePoints(string restorePointDirectoryPath, params FolderConfig[] folderConfigs)
        {
            foreach (var folderConfig in folderConfigs)
                CreateRestorePoint(restorePointDirectoryPath, folderConfig);
        }

        private void CreateRestorePoint(string restorePointDirectoryPath, FolderConfig folderConfig) => folderConfig.CreateRestorePoint(restorePointDirectoryPath, _logger);

        /// <summary>
        /// Sync Folders (It will not work correctly) Todo???
        /// </summary>
        /// <param name="folderConfigs"></param>
        public void SyncFolders(IEnumerable<FolderConfig> folderConfigs, string restorePointDirectoryPath)
        {
            var configs = folderConfigs.ToList();
            if (configs.Count <= 1)
                return;

            var currConfig = configs.First();
            for (int i = 1; i < configs.Count; i++)
            {
                SyncFolders(currConfig, configs[i], restorePointDirectoryPath);
                currConfig = configs[i];
            }
        }

        /// <summary>
        /// Dummy Async method ???todo
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="folderConfig1"></param>
        /// <param name="folderConfig2"></param>
        /// <returns></returns>
        public async Task SyncFoldersAsync(string syncFoldersPath, string restorePointDirectoryPath, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Searching files to sync");

            try
            {
                foreach (var foldersSyncConfig in _foldersSyncReader.ReadFoldersSyncConfigs(syncFoldersPath))
                    SyncFolders(foldersSyncConfig.FolderConfigs, restorePointDirectoryPath);
            }
            catch(Exception exc)
            {
                _logger.LogError(exc, $"Unhandeled Exception when syncing");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Remove unnecessary OperationEvents from both Lists
        /// </summary>
        /// <param name="operationEvents1"></param>
        /// <param name="folderPath1"></param>
        /// <param name="operationEvents2"></param>
        /// <param name="folderPath2"></param>
        private void FilterOperationEvents(List<OperationEvent> operationEvents1, string folderPath1, List<OperationEvent> operationEvents2, string folderPath2)
        {
            if (operationEvents1.Count == 0 && operationEvents2.Count == 0)
                return;

            _logger.LogDebug("Filtering OperationEvents for folders: {folderPath1} and {folderPath2}",folderPath1, folderPath2);

            var dic = new Dictionary<string, List<OperationEvent>>();

            void AddToDic(string folderPath, OperationEvent ev, bool inFirstList)
            {
                string filePath = ev is RenameOperationEvent renOp ? renOp.OldFilePath.SubtractPath(folderPath) : ev.FilePath.SubtractPath(folderPath);

                if (dic.ContainsKey(filePath))
                    dic[filePath].Add(ev);
                else dic.Add(filePath, new List<OperationEvent> { ev });
            }

            foreach (var ev in operationEvents1)
                AddToDic(folderPath1, ev, true);

            foreach (var ev in operationEvents2)
                AddToDic(folderPath2, ev, false);

            var neededOpEvents = new List<OperationEvent>();
            foreach (var pair in dic)
                neededOpEvents.AddRange(pair.Value.FilteredOperationEvents());
            //neededOpEvents.Add(pair.Value.OrderBy(t => t.RaisedTime).Last());

            operationEvents1.RemoveAll(op => !neededOpEvents.Contains(op));
            operationEvents2.RemoveAll(op => !neededOpEvents.Contains(op));
        }
      
        private IEnumerable<(DateTime DateTime, IEnumerable<OperationEvent> OperationEvents)> GetOperationEventsFromFolderConfig(FolderConfig folderConfig)
        {
            bool isFirstTime = true;

            foreach (var filePath in folderConfig.GetFolderConfigJsonLogs())
            {
                if (isFirstTime)
                {
                    _logger.LogDebug("Getting OperationEvents from folder {folderPath}", folderConfig.FolderPath);
                    isFirstTime = false;
                }
                string fileName = Path.GetFileName(filePath);
                var dateTime = fileName.GetCustomDateTime();
                yield return (dateTime, _folderOperationEventsReader.ReadOperationEvents(filePath));
            }
        }
    }
}
