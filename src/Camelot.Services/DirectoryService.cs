using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camelot.Extensions;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.EventArgs;

namespace Camelot.Services
{
    public class DirectoryService : IDirectoryService
    {
        private readonly IPathService _pathService;

        private string _directory;

        public string SelectedDirectory
        {
            get => _directory;
            set
            {
                if (_directory == value)
                {
                    return;
                }

                _directory = value;

                var args = new SelectedDirectoryChangedEventArgs(_directory);
                SelectedDirectoryChanged.Raise(this, args);
            }
        }

        public event EventHandler<SelectedDirectoryChangedEventArgs> SelectedDirectoryChanged;

        public DirectoryService(IPathService pathService)
        {
            _pathService = pathService;
        }

        public bool Create(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public long CalculateSize(string directory) =>
            Directory
                .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

        public DirectoryModel GetDirectory(string directory) => CreateFrom(directory);

        public DirectoryModel GetParentDirectory(string directory)
        {
            var parentDirectory = new DirectoryInfo(directory).Parent;

            return parentDirectory is null ? null : CreateFrom(parentDirectory);
        }

        public IReadOnlyList<DirectoryModel> GetChildDirectories(string directory)
        {
            var directories = Directory
                .GetDirectories(directory)
                .Select(CreateFrom);

            return directories.ToArray();
        }

        public IReadOnlyList<string> GetEmptyDirectoriesRecursive(string directory)
        {
            if (CheckIfEmpty(directory))
            {
                return new[] {directory};
            }

            var directories = GetDirectoriesRecursively(directory);

            return directories.Where(CheckIfEmpty).ToArray();
        }

        public bool CheckIfExists(string directory) => Directory.Exists(directory);

        public string GetAppRootDirectory() => _pathService.GetPathRoot(Directory.GetCurrentDirectory());

        public IEnumerable<string> GetFilesRecursively(string directory) => Directory
                .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .ToArray();

        public IEnumerable<string> GetDirectoriesRecursively(string directory) => Directory
            .EnumerateDirectories(directory, "*.*", SearchOption.AllDirectories)
            .ToArray();

        public void RemoveRecursively(string directory) => Directory.Delete(directory, true);

        public bool Rename(string directoryPath, string newName)
        {
            var parentDirectory = _pathService.GetParentDirectory(directoryPath);
            var newDirectoryPath = _pathService.Combine(parentDirectory, newName);

            try
            {
                Directory.Move(directoryPath, newDirectoryPath);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool CheckIfEmpty(string directory) =>
            !Directory.EnumerateFileSystemEntries(directory).Any();

        private static DirectoryModel CreateFrom(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);

            return CreateFrom(directoryInfo);
        }

        private static DirectoryModel CreateFrom(FileSystemInfo directoryInfo) =>
            new DirectoryModel
            {
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                LastModifiedDateTime = directoryInfo.LastWriteTime,
                LastAccessDateTime = directoryInfo.LastAccessTime,
                CreatedDateTime = directoryInfo.CreationTime
            };
    }
}