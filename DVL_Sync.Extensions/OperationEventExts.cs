using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DVL_Sync.Extensions
{
    public static class OperationEventExts
    {
        public class OperationsTree
        {
            //public string val { get { return @event.} }
            public OperationEvent @event { get; set; }
            public List<OperationsTree> childsTree { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationEvents"></param>
        /// <returns></returns>
        public static IEnumerable<OperationEvent> FilteredOperationEvents(
            this IEnumerable<OperationEvent> operationEvents)
        {
            var filteredOperations = operationEvents.Where(opEvent => !(opEvent.EventType == EventType.Edit && opEvent.FileType == FileType.Directory)).OrderBy(opEvent=>opEvent.RaisedTime);

            var rootFolder =  new FolderViewModel(filteredOperations.GetRootPath());

            foreach (var opEvent in filteredOperations)
            {
                if (opEvent.FileType == FileType.Directory)
                    rootFolder.AddFolderToRootFolder(opEvent);
                else rootFolder.AddFileToRootFolder(opEvent);
            }

            return rootFolder.GetOperationEventsFromRootFolderViewModel().OrderBy(opEvent => opEvent.RaisedTime);

            var dic = new Dictionary<string, List<OperationEvent>>();
            foreach (var filOp in filteredOperations)
            {
                if (filOp is RenameOperationEvent renameOp && dic.ContainsKey(renameOp.OldFilePath))
                {
                    dic[renameOp.OldFilePath].Add(filOp);
                    continue;
                }

                if (dic.ContainsKey(filOp.FilePath))
                    dic[filOp.FilePath].Add(filOp);
                else dic.Add(filOp.FilePath, new List<OperationEvent> {filOp});
            }

            foreach (var pair in dic)
            {
                //Deleting Redundant Events Before Delete Event
                int createEventIndex = pair.Value.FindIndex(op => op.EventType == EventType.Create);
                int firstDeleteEventIndex = pair.Value.FindIndex(op => op.EventType == EventType.Delete);
                int lastDeleteEventIndex = pair.Value.FindLastIndex(op => op.EventType == EventType.Delete);
                DateTime? deletedTime = null;
                if (lastDeleteEventIndex >= 0)
                {
                    deletedTime = pair.Value[lastDeleteEventIndex].RaisedTime;
                    if (createEventIndex < firstDeleteEventIndex && createEventIndex >= 0)
                        pair.Value.RemoveRange(0, lastDeleteEventIndex + 1);
                    else pair.Value.RemoveRange(0, lastDeleteEventIndex);
                }

                if (createEventIndex >= 0)
                {
                    //If Create Event Presents All Edit Events Will be Deleted (In this Situation I need Copy of this File Already)
                    var last = pair.Value.LastOrDefault(p => p.EventType == EventType.Edit);
                    if (last != null)
                        pair.Value[createEventIndex].RaisedTime = last.RaisedTime;

                    //If Rename happened I need Renamed FilePath, because CopyOperation will not work on Previous FilePath
                    var lastRename = pair.Value.LastOrDefault(p => p.EventType == EventType.Rename);
                    if (lastRename != null)
                    {
                        pair.Value[createEventIndex].FilePath = lastRename.FilePath;
                        pair.Value.RemoveAll(p => p.EventType == EventType.Rename);
                    }

                    pair.Value.RemoveAll(p => p.EventType == EventType.Edit);
                }

                //If Directory was Deleted
                if (pair.Value.Any(p => p.FileType == FileType.Directory) && deletedTime != null)
                    foreach (var operationsList in dic.Where(p => p.Key.IndexOf(pair.Key) == 0).Select(p => p.Value))
                        operationsList.RemoveAll(op => op.RaisedTime <= deletedTime);
            }

            return dic.SelectMany(p => p.Value).OrderBy(ev => ev.RaisedTime);
        }

        public static IEnumerable<Operation> GetOperations(this IEnumerable<OperationEvent> operationEvents, IOperationFactory<OperationEvent> factory)
        {
            foreach (var operationEvent in operationEvents)
                yield return factory.CreateOperation(operationEvent);
        }

        public static string GetRootPath(this IEnumerable<OperationEvent> operationEvents)
        {
            var splitted = operationEvents.First().FilePath.Split(new[] {"\\"}, StringSplitOptions.None);
            string split = splitted[0];
            int i = 2;
            while (i <= splitted.Length && operationEvents.All(op => op.FilePath.Contains(split)))
                split = string.Join("\\", splitted.Take(i++));

            return string.Join("\\", splitted.Take(i - 1));
        }

        public static IEnumerable<OperationEvent> GetOperationEventsFromRootFolderViewModel(
            this FolderViewModel rootFolder, string path = null)
        {
            if (path == null)
                path = rootFolder.Name;

            if (rootFolder.OperationEvent != null)
                yield return RefactorOperationEvent(rootFolder.OperationEvent);

            foreach (var file in rootFolder.Files)
                yield return RefactorOperationEvent(file.OperationEvent);

            if (rootFolder.OperationEvent != null && !(rootFolder.OperationEvent is DeleteOperationEvent))
            {
                foreach (var folder in rootFolder.Folders)
                foreach (var opEvent in folder.GetOperationEventsFromRootFolderViewModel(Path.Combine(path, folder.Name)))
                    yield return opEvent;
            }

            OperationEvent RefactorOperationEvent(OperationEvent opEvent)
            {
                opEvent.FilePath = Path.Combine(path, opEvent.FileName);
                return opEvent;
            }
        }

        public static void AddFileToRootFolder(this FolderViewModel rootFolder, OperationEvent opEvent)
        {
            var currFolder = rootFolder;
            var splitted = opEvent.FilePath.Split(new[] {"\\"}, StringSplitOptions.None);
            for (int i = 1; i < splitted.Length - 1; i++)
            {
                var fold = currFolder.Folders.FirstOrDefault(folder => folder.Name == splitted[i]);
                if (fold == null)
                {
                    currFolder.Folders.Add(new FolderViewModel(splitted[i]));
                    currFolder = currFolder.Folders[currFolder.Folders.Count - 1];
                }
                else currFolder = fold;
            }

            string fileName = splitted[splitted.Length - 1];
            var file = currFolder.Files.FirstOrDefault(fl => fl.Name == fileName);
            if (file == null)
                currFolder.Files.Add(new FileViewModel(fileName) {OperationEvent = opEvent});
            else
            {
                switch (opEvent)
                {
                    case RenameOperationEvent renOpEvent:
                        file.Name = renOpEvent.FileName;
                        file.OperationEvent = renOpEvent;
                        break;
                    case EditOperationEvent editOpEvent:
                        file.OperationEvent = editOpEvent;
                        break;
                    case DeleteOperationEvent deleteOpEvent:
                        file.OperationEvent = deleteOpEvent;
                        break;
                    default: throw new NotImplementedException("CreateOperationEvent not implemented");
                }
            }
        }

        public static void AddFolderToRootFolder(this FolderViewModel rootFolder, OperationEvent opEvent)
        {
            var currFolder = rootFolder;
            var splitted = opEvent.FilePath.Split(new[] { "\\" }, StringSplitOptions.None);
            for (int i = 1; i < splitted.Length - 1; i++)
            {
                var fold = currFolder.Folders.FirstOrDefault(f => f.Name == splitted[i]);
                if (fold == null)
                {
                    currFolder.Folders.Add(new FolderViewModel(splitted[i]));
                    currFolder = currFolder.Folders[currFolder.Folders.Count - 1];
                }
                else currFolder = fold;
            }

            string folderName = splitted[splitted.Length - 1];
            var folder = currFolder.Folders.FirstOrDefault(fold => fold.Name == folderName);
            if (folder == null)
                currFolder.Folders.Add(new FolderViewModel(folderName) { OperationEvent = opEvent });
            else
            {
                switch (opEvent)
                {
                    case RenameOperationEvent renOpEvent:
                        folder.Name = renOpEvent.FileName;
                        folder.OperationEvent = renOpEvent;
                        break;
                    case DeleteOperationEvent deleteOpEvent:
                        folder.OperationEvent = deleteOpEvent;
                        break;
                    default: throw new NotImplementedException("CreateOperationEvent or EditOperationEvent not implemented");
                }
            }
        }
    }
}
