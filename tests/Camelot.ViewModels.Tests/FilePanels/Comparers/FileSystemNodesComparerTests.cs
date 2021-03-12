using System;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Archive;
using Camelot.Services.Abstractions.Behaviors;
using Camelot.Services.Abstractions.Models.Enums;
using Camelot.Services.Abstractions.Operations;
using Camelot.ViewModels.Implementations.MainWindow.FilePanels;
using Camelot.ViewModels.Implementations.MainWindow.FilePanels.Comparers;
using Camelot.ViewModels.Interfaces.Behaviors;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels;
using Camelot.ViewModels.Services.Interfaces;
using Moq.AutoMock;
using Xunit;

namespace Camelot.ViewModels.Tests.FilePanels.Comparers
{
    public class FileSystemNodesComparerTests
    {
        private readonly AutoMocker _autoMocker;

        public FileSystemNodesComparerTests()
        {
            _autoMocker = new AutoMocker();
        }

        [Theory]
        [InlineData(true, SortingMode.Date)]
        [InlineData(false, SortingMode.Name)]
        [InlineData(true, SortingMode.Extension)]
        [InlineData(false, SortingMode.Size)]
        public void TestSortingParentDirectory(bool isAscending, SortingMode sortingColumn)
        {
            _autoMocker.Use(true);

            var parentDirectoryViewModel =  _autoMocker.CreateInstance<DirectoryViewModel>();
            parentDirectoryViewModel.IsParentDirectory = true;
            var directoryViewModel =  _autoMocker.CreateInstance<DirectoryViewModel>();

            var comparer = new FileSystemNodesComparer(isAscending, sortingColumn);

            var result = comparer.Compare(parentDirectoryViewModel, directoryViewModel);
            Assert.True(result < 0);

            result = comparer.Compare(directoryViewModel, parentDirectoryViewModel);
            Assert.True(result > 0);
        }

        [Theory]
        [InlineData(true, SortingMode.Date)]
        [InlineData(false, SortingMode.Name)]
        [InlineData(true, SortingMode.Extension)]
        [InlineData(false, SortingMode.Size)]
        public void TestSortingFileAndDirectory(bool isAscending, SortingMode sortingColumn)
        {
            _autoMocker.Use(true);

            var directoryViewModel =  _autoMocker.CreateInstance<DirectoryViewModel>();
            var fileViewModel =  _autoMocker.CreateInstance<FileViewModel>();

            var comparer = new FileSystemNodesComparer(isAscending, sortingColumn);

            var result = comparer.Compare(directoryViewModel, fileViewModel);
            Assert.True(result < 0);

            result = comparer.Compare(fileViewModel, directoryViewModel);
            Assert.True(result > 0);
        }

        [Theory]
        [InlineData(true, SortingMode.Date)]
        [InlineData(false, SortingMode.Name)]
        [InlineData(true, SortingMode.Extension)]
        [InlineData(false, SortingMode.Size)]
        public void TestThrows(bool isAscending, SortingMode sortingColumn)
        {
            _autoMocker.Use(true);

            var directoryViewModel =  _autoMocker.CreateInstance<DirectoryViewModel>();
            var nodeViewModel =  _autoMocker.CreateInstance<NodeViewModel>();

            var comparer = new FileSystemNodesComparer(isAscending, sortingColumn);

            void Compare() => comparer.Compare(nodeViewModel, directoryViewModel);

            Assert.Throws<InvalidOperationException>(Compare);

            void CompareReversed() => comparer.Compare(directoryViewModel, nodeViewModel);

            Assert.Throws<InvalidCastException>(CompareReversed);
        }

        private class NodeViewModel : FileSystemNodeViewModelBase
        {
            public NodeViewModel(
                IFileSystemNodeOpeningBehavior fileSystemNodeOpeningBehavior,
                IOperationsService operationsService,
                IClipboardOperationsService clipboardOperationsService,
                IFilesOperationsMediator filesOperationsMediator,
                IFileSystemNodePropertiesBehavior fileSystemNodePropertiesBehavior,
                IDialogService dialogService,
                ITrashCanService trashCanService,
                IArchiveService archiveService,
                ISystemDialogService systemDialogService,
                IOpenWithApplicationService openWithApplicationService,
                IPathService pathService,
                bool shouldShowOpenSubmenu)
                : base(
                    fileSystemNodeOpeningBehavior,
                    operationsService,
                    clipboardOperationsService,
                    filesOperationsMediator,
                    fileSystemNodePropertiesBehavior,
                    dialogService,
                    trashCanService,
                    archiveService,
                    systemDialogService,
                    openWithApplicationService,
                    pathService,
                    shouldShowOpenSubmenu)
            {

            }
        }
    }
}