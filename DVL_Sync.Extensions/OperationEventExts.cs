﻿using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var filteredOperations = operationEvents.Where(opEvent => !(opEvent.EventType == EventType.Edit && opEvent.FileType == FileType.Directory));
            var dic = new ConcurrentDictionary<string, List<OperationEvent>>();
            foreach (var filOp in filteredOperations)
            {
                if (filOp is RenameOperationEvent renameOp && dic.ContainsKey(renameOp.OldFilePath))
                {
                    dic[renameOp.OldFilePath].Add(filOp);
                    continue;
                }

                if (!dic.TryAdd(filOp.FilePath, new List<OperationEvent> { filOp }))
                    dic[filOp.FilePath].Add(filOp);

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
    }
}
