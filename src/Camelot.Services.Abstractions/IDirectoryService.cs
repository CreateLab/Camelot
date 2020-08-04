using System;
using System.Collections.Generic;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.Abstractions.Models.EventArgs;

namespace Camelot.Services.Abstractions
{
    public interface IDirectoryService
    {
        event EventHandler<SelectedDirectoryChangedEventArgs> SelectedDirectoryChanged;

        string SelectedDirectory { get; set; }

        bool Create(string directory);

        long CalculateSize(string directory);

        DirectoryModel GetDirectory(string directory);

        DirectoryModel GetParentDirectory(string directory);

        IReadOnlyList<DirectoryModel> GetDirectories(IReadOnlyList<string> directories);

        IReadOnlyList<DirectoryModel> GetChildDirectories(string directory);

        bool CheckIfExists(string directory);

        string GetAppRootDirectory();

        IReadOnlyList<string> GetFilesRecursively(string directory);

        void RemoveRecursively(string directory);

        bool Rename(string directoryPath, string newName);
    }
}