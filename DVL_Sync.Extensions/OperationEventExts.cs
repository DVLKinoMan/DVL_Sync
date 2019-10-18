using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DVL_Sync.Extensions
{
    public static class OperationEventExts
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationEvents"></param>
        /// <returns></returns>
        public static IEnumerable<OperationEvent> FilteredOperationEvents(
            this IEnumerable<OperationEvent> operationEvents)
        {
            var filteredOperations = operationEvents
                                                    .Where(opEvent => !(opEvent.EventType == EventType.Edit && opEvent.FileType == FileType.Directory))
                                                    .OrderBy(opEvent=>opEvent.RaisedTime).ToList();

            if (filteredOperations.Count() <= 1)
                return filteredOperations;

            var rootFolder =  new FolderViewModel(filteredOperations.GetRootPath());

            foreach (var opEvent in filteredOperations)
            {
                if (opEvent.FileType == FileType.Directory)
                    rootFolder.AddFolderToRootFolder(opEvent);
                else rootFolder.AddFileToRootFolder(opEvent);
            }

            return rootFolder.GetOperationEventsFromRootFolderViewModel().OrderBy(opEvent => opEvent.RaisedTime);
        }

        public static IEnumerable<Operation> GetOperations(this IEnumerable<OperationEvent> operationEvents, IOperationFactory<OperationEvent> factory)
        {
            foreach (var operationEvent in operationEvents)
                yield return factory.CreateOperation(operationEvent);
        }

        /// <summary>
        /// Get's root folder path for all operationEvents
        /// </summary>
        /// <param name="operationEvents"></param>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException">description</exception>
        /// <exception cref="System.InvalidOperationException">description</exception>
        public static string GetRootPath(this IEnumerable<OperationEvent> operationEvents)
        {
            if (operationEvents == null)
                throw new NullReferenceException("Object reference not set to an instance of an object (operationEvents");
            if (!operationEvents.Any())
                throw new InvalidOperationException("Sequence contains no elements");

            var splitted = operationEvents.First().FilePath.Split(new[] {"\\"}, StringSplitOptions.None);
            string split = string.Empty;
            int i = 1;
            while (i <= splitted.Length && !IsFile(splitted[i-1]) && operationEvents.All(op => op.FilePath.Contains(GetJoinedString(i))))
                split = GetJoinedString(i++);

            return split;

            string GetJoinedString(int k) => string.Join("\\", splitted.Take(k));
            bool IsFile(string splittedEl) => splittedEl.Contains(".txt") || splittedEl.Contains(".json");
        }

        public static IEnumerable<OperationEvent> GetOperationEventsFromRootFolderViewModel(
            this FolderViewModel rootFolder, string path = null)
        {
            if (path == null)
                path = rootFolder.Name;

            if (rootFolder.OperationEvents != null)
                foreach (var opEvent in RefactorOperationEvents(rootFolder.OperationEvents, true))
                    yield return opEvent;

            //foreach (var file in rootFolder.Files)
            //    foreach (var opEvent in RefactorOperationEvents(file.OperationEvents))
            //        yield return opEvent;
            foreach (var file in rootFolder.Files)
                foreach (var opEvent in RemoveUnnecessaryOperationEvents(file))
                    yield return opEvent;

            //If there is DeleteOperationEvent and then another operationevents for innerfolders there might be bug
            if (rootFolder.OperationEvents.Count == 0 || !(rootFolder.OperationEvents.Last() is DeleteOperationEvent))
                foreach (var folder in rootFolder.Folders)
                    foreach (var opEvent in folder.GetOperationEventsFromRootFolderViewModel(Path.Combine(path, folder.Name)))
                        yield return opEvent;

            IEnumerable<OperationEvent> RefactorOperationEvents(IEnumerable<OperationEvent> opEvents, bool isRootFolderEvent = false)
            {
                foreach (var opEvent in opEvents)
                    yield return RefactorOperationEvent(opEvent, isRootFolderEvent);
            }

            OperationEvent RefactorOperationEvent(OperationEvent opEvent, bool isRootFolderEvent = false)
            {
                opEvent.FilePath = !isRootFolderEvent ? Path.Combine(path, opEvent.FileName) : path;

                return opEvent;
            }

            IEnumerable<OperationEvent> RemoveUnnecessaryOperationEvents(FileViewModel fileViewModel)
            {
                if (fileViewModel.OperationEvents.Any(op => op.EventType == EventType.Create))
                    fileViewModel.OperationEvents.RemoveAll(op => op.EventType == EventType.Edit);
                else  fileViewModel.OperationEvents.RemoveAllExceptLast(op => op.EventType == EventType.Edit, 1);

                return fileViewModel.OperationEvents;
            }
        }

        public static bool AddFileToRootFolder(this FolderViewModel rootFolder, OperationEvent opEvent)
        {
            var currFolder = rootFolder;
            var splitted = SplitWithSlashes(opEvent.FilePath);

            if (rootFolder.Name != opEvent.FilePath.Substring(0, rootFolder.Name.Length))
                return false;

            for (int i = SplitWithSlashes(rootFolder.Name).Length; i < splitted.Length - 1; i++)
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
            if (opEvent is RenameOperationEvent renOpEvent)
            {
                //currFolder.Files.RemoveAll(fl => fl.Name == renOpEvent.OldFileName);

                var oldFile = currFolder.Files.FirstOrDefault(fl => fl.Name == renOpEvent.OldFileName);
                if (oldFile != null)
                {
                    //oldFile.AddOperationEvent(opEvent);
                    oldFile.Name = renOpEvent.FileName;
                    if (oldFile.OperationEvents.Count != 0)
                    {
                        foreach (var op in oldFile.OperationEvents)
                            op.FilePath = op.FilePath.Substring(0, op.FilePath.Length - op.FileName.Length) + renOpEvent.FileName;
                    }
                    else oldFile.AddOperationEvent(opEvent);
                }
                else currFolder.Files.Add(new FileViewModel(fileName).AddOperationEvent(opEvent));
            }
            else
            {
                if (file == null)
                    currFolder.Files.Add(new FileViewModel(fileName).AddOperationEvent(opEvent));
                else
                {
                    switch (opEvent)
                    {
                        //case RenameOperationEvent renOpEvent:
                        //    file.Name = renOpEvent.FileName;
                        //    file.OperationEvent = renOpEvent;
                        //    break;
                        case EditOperationEvent editOpEvent:
                            if (!file.OperationEvents.Any(op => op.EventType == EventType.Create || op.EventType == EventType.Edit))
                                file.AddOperationEvent(editOpEvent);
                            break;
                        case DeleteOperationEvent deleteOpEvent:
                            if (file.OperationEvents.Any(op => op.EventType == EventType.Create))
                                file.OperationEvents = new List<OperationEvent>();
                            else
                            {
                                file.OperationEvents = new List<OperationEvent>();
                                file.AddOperationEvent(deleteOpEvent);
                            }
                            break;
                        default: throw new NotImplementedException("CreateOperationEvent not implemented");
                    }
                }
            }

            return true;

            string[] SplitWithSlashes(string str) => str.Split(new[] { "\\" }, StringSplitOptions.None);
        }

        public static bool AddFolderToRootFolder(this FolderViewModel rootFolder, OperationEvent opEvent)
        {
            var currFolder = rootFolder;
            var splitted = SplitWithSlashes(opEvent.FilePath);

            if (rootFolder.Name != opEvent.FilePath.Substring(0, rootFolder.Name.Length))
                return false;

            for (int i = SplitWithSlashes(rootFolder.Name).Length; i < splitted.Length - 1; i++)
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
            if (opEvent is RenameOperationEvent renOpEvent)
            {
                var oldFolder = currFolder.Folders.FirstOrDefault(fold => fold.Name == renOpEvent.OldFileName);
                if (oldFolder != null)
                {
                    oldFolder.Name = folderName;
                    //oldFolder.OperationEvents = opEvent;
                    if (!oldFolder.OperationEvents.Any(op => op.EventType == EventType.Create || op.EventType == EventType.Edit))
                        oldFolder.AddOperationEvent(opEvent);
                }
                else currFolder.Folders.Add(new FolderViewModel(folderName).AddOperationEvent(opEvent));
            }
            else
            {
                if (folder == null)
                    currFolder.Folders.Add(new FolderViewModel(folderName).AddOperationEvent(opEvent));
                else
                {
                    switch (opEvent)
                    {
                        //case RenameOperationEvent renOpEvent:
                        //    folder.Name = renOpEvent.FileName;
                        //    folder.OperationEvent = renOpEvent;
                        //    break;
                        case DeleteOperationEvent deleteOpEvent:
                            if (folder.OperationEvents.Any(op => op.EventType == EventType.Create))
                                folder.OperationEvents = new List<OperationEvent>();
                            else
                            {
                                folder.OperationEvents = new List<OperationEvent>();
                                folder.AddOperationEvent(deleteOpEvent);
                            }
                            break;
                        default: throw new NotImplementedException("CreateOperationEvent or EditOperationEvent not implemented");
                    }
                }
            }

            return true;

            string[] SplitWithSlashes(string str) => str.Split(new[] { "\\" }, StringSplitOptions.None);
        }
    }
}
