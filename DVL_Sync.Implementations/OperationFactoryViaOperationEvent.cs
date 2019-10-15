using DVL_Sync.Abstractions;
using DVL_Sync.Models;
using DVL_Sync_FileEventsLogger.Models;
using System;
using System.Extensions;

namespace DVL_Sync.Implementations
{
    public class OperationFactoryViaOperationEvent : IOperationFactory<OperationEvent>
    {
        private readonly string _folderRootPath;

        public OperationFactoryViaOperationEvent(string folderRootPath) =>
            _folderRootPath = folderRootPath;

        public Operation CreateOperation(OperationEvent opEvent) => opEvent switch
        {
            CreateOperationEvent createOperationEvent when createOperationEvent.FileType == FileType.Directory => CreateDirectoryOperation(createOperationEvent),
            CreateOperationEvent createOperationEvent => CreateCopyOperation(createOperationEvent) as Operation,
            EditOperationEvent editOperationEvent => CreateCopyOperation(editOperationEvent),
            DeleteOperationEvent deleteOperationEvent => CreateDeleteOperation(deleteOperationEvent),
            RenameOperationEvent renameOperationEvent => CreateRenameOperation(renameOperationEvent),
            _ => throw new NotImplementedException("OperationEvent not implemented")
        };

        private CreateDirectoryOperation CreateDirectoryOperation(CreateOperationEvent opEvent) =>
            new CreateDirectoryOperation
            {
                DirectoryPathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath)
            };

        private CopyOperation CreateCopyOperation(CreateOperationEvent opEvent) =>
            new CopyOperation
            {
                FilePathToCopy = opEvent.FilePath,
                FilePathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath)
                //DirectoryPathToPaste = opEvent.
            };

        private CopyOperation CreateCopyOperation(EditOperationEvent opEvent) =>
        new CopyOperation
        {
            FilePathToCopy = opEvent.FilePath,
            FilePathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath)
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
            //FilePathFromRoot = opEvent.FilePath.SubtractPath(_folderRootPath),
            FilePathFromRoot = opEvent.OldFilePath.SubtractPath(_folderRootPath),
            NewName = opEvent.FileName
        };
    }
}
