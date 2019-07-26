using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Extensions;

namespace DVL_Sync.Implementations
{
    public class OperationFactoryViaOperationEvent : IOperationFactory<OperationEvent>
    {
        private string _folderRootPath;

        public OperationFactoryViaOperationEvent(string folderRootPath)
        {
            this._folderRootPath = folderRootPath;
        }

        public Operation CreateOperation(OperationEvent opEvent) => opEvent switch
        {
            CreateOperationEvent createOperationEvent => CreateCopyOperation(createOperationEvent) as Operation,
            EditOperationEvent editOperationEvent => CreateCopyOperation(editOperationEvent),
            DeleteOperationEvent deleteOperationEvent => CreateDeleteOperation(deleteOperationEvent),
            RenameOperationEvent renameOperationEvent => CreateRenameOperation(renameOperationEvent),
            _ => throw new NotImplementedException("OperationEvent not implemented")
        };

        private CopyOperation CreateCopyOperation(CreateOperationEvent opEvent) =>
            new CopyOperation
            {
                FilePathToCopy = opEvent.FilePath,
                //DirectoryPathToPaste = opEvent.
            };

        private CopyOperation CreateCopyOperation(EditOperationEvent opEvent) =>
        new CopyOperation
        {
            FilePathToCopy = opEvent.FilePath,
            //DirectoryPathToPaste = opEvent.
        };

        private DeleteOperation CreateDeleteOperation(DeleteOperationEvent opEvent) =>
        new DeleteOperation
        {
            FilePathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath)
        };

        private RenameOperation CreateRenameOperation(RenameOperationEvent opEvent) =>
        new RenameOperation
        {
            FilePathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath),
            NewName = opEvent.FileName
        };
    }
}
