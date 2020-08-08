using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camelot.Extensions;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models.Operations;
using Camelot.Services.Abstractions.Operations;

namespace Camelot.Operations
{
    public class OperationsService : IOperationsService
    {
        private readonly IOperationsFactory _operationsFactory;
        private readonly IDirectoryService _directoryService;
        private readonly IResourceOpeningService _resourceOpeningService;
        private readonly IFileService _fileService;
        private readonly IPathService _pathService;
        private readonly IOperationsStateService _operationsStateService;

        public OperationsService(
            IOperationsFactory operationsFactory,
            IDirectoryService directoryService,
            IResourceOpeningService resourceOpeningService,
            IFileService fileService,
            IPathService pathService,
            IOperationsStateService operationsStateService)
        {
            _operationsFactory = operationsFactory;
            _directoryService = directoryService;
            _resourceOpeningService = resourceOpeningService;
            _fileService = fileService;
            _pathService = pathService;
            _operationsStateService = operationsStateService;
        }

        public void OpenFiles(IReadOnlyList<string> files) => files.ForEach(_resourceOpeningService.Open);

        public async Task CopyAsync(IReadOnlyList<string> nodes, string destinationDirectory)
        {
            var settings = GetBinaryFileSystemOperationSettings(nodes, destinationDirectory);
            var copyOperation = _operationsFactory.CreateCopyOperation(settings);
            _operationsStateService.AddOperation(copyOperation);

            await copyOperation.RunAsync();
        }

        public async Task MoveAsync(IReadOnlyList<string> nodes, string destinationDirectory)
        {
            var settings = GetBinaryFileSystemOperationSettings(nodes, destinationDirectory);
            var moveOperation = _operationsFactory.CreateMoveOperation(settings);
            _operationsStateService.AddOperation(moveOperation);

            await moveOperation.RunAsync();
        }

        public async Task MoveAsync(IReadOnlyDictionary<string, string> nodes)
        {
            var settings = GetBinaryFileSystemOperationSettings(nodes);
            var moveOperation = _operationsFactory.CreateMoveOperation(settings);
            _operationsStateService.AddOperation(moveOperation);

            await moveOperation.RunAsync();
        }

        public async Task RemoveAsync(IReadOnlyList<string> nodes)
        {
            var (files, directories) = Split(nodes);
            var sourceDirectory = GetCommonRootDirectory(nodes);
            var settings = Create(files, directories, sourceDirectory);
            var deleteOperation = _operationsFactory.CreateDeleteOperation(settings);
            _operationsStateService.AddOperation(deleteOperation);

            await deleteOperation.RunAsync();
        }

        public bool Rename(string path, string newName)
        {
            if (_fileService.CheckIfExists(path))
            {
                return _fileService.Rename(path, newName);
            }

            return _directoryService.CheckIfExists(path) && _directoryService.Rename(path, newName);
        }

        public void CreateDirectory(string sourceDirectory, string directoryName)
        {
            var fullPath = _pathService.Combine(sourceDirectory, directoryName);

            _directoryService.Create(fullPath);
        }

        private (string[] Files, string[] Directories) Split(IReadOnlyList<string> nodes)
        {
            var files = nodes
                .Where(_fileService.CheckIfExists)
                .ToArray();
            var directories = nodes
                .Where(_directoryService.CheckIfExists)
                .ToArray();

            return (files, directories);
        }

        private BinaryFileSystemOperationSettings GetBinaryFileSystemOperationSettings(
            IReadOnlyList<string> nodes,
            string outputDirectory)
        {
            var (files, directories) = Split(nodes);
            var sourceDirectory = GetCommonRootDirectory(nodes);
            var filesInDirectories = directories.SelectMany(_directoryService.GetFilesRecursively);
            var emptyDirectories = directories
                .SelectMany(_directoryService.GetEmptyDirectoriesRecursively)
                .Select(d => GetDestinationPath(sourceDirectory, d, outputDirectory))
                .ToArray();
            var filePathsDictionary = filesInDirectories
                .Concat(files)
                .ToDictionary(
                    f => f,
                    f => GetDestinationPath(sourceDirectory, f, outputDirectory));
            var outputTopLevelFiles = files
                .Select(f => GetDestinationPath(sourceDirectory, f, outputDirectory))
                .ToArray();
            var outputTopLevelDirectories = directories
                .Select(f => GetDestinationPath(sourceDirectory, f, outputDirectory))
                .ToArray();

            return new BinaryFileSystemOperationSettings(directories, files, outputTopLevelDirectories,
                outputTopLevelFiles, filePathsDictionary, emptyDirectories, sourceDirectory, outputDirectory);
        }

        private BinaryFileSystemOperationSettings GetBinaryFileSystemOperationSettings(
            IReadOnlyDictionary<string, string> nodes)
        {
            var (files, directories) = Split(nodes.Keys.ToArray());
            var sourceDirectory = GetCommonRootDirectory(nodes.Keys.ToArray());

            var emptyDirectories = new List<string>();
            var filePathsDictionary = files.ToDictionary(f => f, f => nodes[f]);
            foreach (var directory in directories)
            {
                var filesInDirectory = _directoryService.GetFilesRecursively(directory);
                var outputDirectory = nodes[directory];

                filesInDirectory.ForEach(f =>
                    filePathsDictionary[f] = GetDestinationPath(sourceDirectory, f, outputDirectory));
                var innerEmptyDirectories = _directoryService
                    .GetEmptyDirectoriesRecursively(directory)
                    .Select(d => GetDestinationPath(sourceDirectory, d, outputDirectory));
                emptyDirectories.AddRange(innerEmptyDirectories);
            }

            var outputTopLevelFiles = files
                .Select(f => nodes[f])
                .ToArray();
            var outputTopLevelDirectories = directories
                .Select(f => nodes[f])
                .ToArray();

            return new BinaryFileSystemOperationSettings(directories, files, outputTopLevelDirectories,
                outputTopLevelFiles, filePathsDictionary, emptyDirectories, sourceDirectory);
        }

        private string GetCommonRootDirectory(IReadOnlyList<string> nodes) =>
            _pathService.GetCommonRootDirectory(nodes);

        private string GetDestinationPath(string sourceDirectory,
            string sourcePath, string destinationDirectory)
        {
            var relativeSourcePath = _pathService.GetRelativePath(sourceDirectory, sourcePath);

            return _pathService.Combine(destinationDirectory, relativeSourcePath);
        }

        private static UnaryFileSystemOperationSettings Create(
            IReadOnlyList<string> files, IReadOnlyList<string> directories, string sourceDirectory) =>
            new UnaryFileSystemOperationSettings(directories, files, sourceDirectory);
    }
}