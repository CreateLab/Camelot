using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Camelot.Extensions;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Operations;
using Camelot.Services.AllPlatforms;
using Camelot.Services.Environment.Interfaces;
using Camelot.Services.Linux.Interfaces.Builders;

namespace Camelot.Services.Linux
{
    public class LinuxTrashCanService : TrashCanServiceBase
    {
        private readonly IPathService _pathService;
        private readonly IFileService _fileService;
        private readonly IDirectoryService _directoryService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILinuxRemovedFileMetadataBuilderFactory _linuxRemovedFileMetadataBuilderFactory;
        private readonly IEnvironmentService _environmentService;

        public LinuxTrashCanService(
            IDriveService driveService,
            IOperationsService operationsService,
            IPathService pathService,
            IFileService fileService,
            IEnvironmentService environmentService,
            IDirectoryService directoryService,
            IDateTimeProvider dateTimeProvider,
            ILinuxRemovedFileMetadataBuilderFactory linuxRemovedFileMetadataBuilderFactory)
            : base(driveService, operationsService, pathService, fileService)
        {
            _pathService = pathService;
            _fileService = fileService;
            _environmentService = environmentService;
            _directoryService = directoryService;
            _dateTimeProvider = dateTimeProvider;
            _linuxRemovedFileMetadataBuilderFactory = linuxRemovedFileMetadataBuilderFactory;
        }

        protected override IReadOnlyCollection<string> GetTrashCanLocations(string volume)
        {
            var directories = new List<string>();
            if (volume != "/")
            {
                directories.AddRange(GetVolumeTrashCanPaths(volume));
            }

            directories.Add(GetHomeTrashCanPath());

            return directories;
        }

        protected override string GetFilesTrashCanLocation(string trashCanLocation) =>
            $"{trashCanLocation}/files";

        protected override async Task WriteMetaDataAsync(IReadOnlyDictionary<string, string> filePathsDictionary,
            string trashCanLocation)
        {
            var infoTrashCanLocation = GetInfoTrashCanLocation(trashCanLocation);
            if (!_directoryService.CheckIfExists(infoTrashCanLocation))
            {
                _directoryService.Create(infoTrashCanLocation);
            }

            var deleteTime = _dateTimeProvider.Now;

            await filePathsDictionary.Keys.ForEachAsync(f => WriteMetaDataAsync(f, infoTrashCanLocation, deleteTime));
        }

        protected override string GetUniqueFilePath(string file, HashSet<string> filesSet, string directory)
        {
            var filePath = _pathService.Combine(directory, _pathService.GetFileName(file));
            if (!filesSet.Contains(filePath))
            {
                return filePath;
            }

            var fileName = _pathService.GetFileName(file);

            string result;
            var i = 1;
            do
            {
                var newFileName = $"{fileName} ({i})";
                result = _pathService.Combine(directory, newFileName);
                i++;
            } while (filesSet.Contains(result) || _fileService.CheckIfExists(result));

            return result;
        }

        private async Task WriteMetaDataAsync(string file, string trashCanMetadataLocation, DateTime dateTime)
        {
            var fileName = _pathService.GetFileName(file);
            var metadataFullPath = _pathService.Combine(trashCanMetadataLocation, fileName + ".trashinfo");
            var metadata = GetMetadata(file, dateTime);

            await _fileService.WriteTextAsync(metadataFullPath, metadata);
        }

        private string GetMetadata(string filePath, DateTime dateTime)
        {
            var builder = CreateBuilder()
                .WithFilePath(filePath)
                .WithRemovingDateTime(dateTime);

            return builder.Build();
        }

        private ILinuxRemovedFileMetadataBuilder CreateBuilder() =>
            _linuxRemovedFileMetadataBuilderFactory.Create();

        private static string GetInfoTrashCanLocation(string trashCanLocation) =>
            $"{trashCanLocation}/info";

        private string GetHomeTrashCanPath()
        {
            var xdgDataHome = _environmentService.GetEnvironmentVariable("XDG_DATA_HOME");
            if (xdgDataHome != null)
            {
                return $"{xdgDataHome}/Trash/";
            }

            var home = _environmentService.GetEnvironmentVariable("HOME");

            return $"{home}/.local/share/Trash";
        }

        private IReadOnlyCollection<string> GetVolumeTrashCanPaths(string volume)
        {
            var uid = GetUid();

            return new[] {$"{volume}/.Trash-{uid}", $"{volume}/.Trash/{uid}"};
        }

        private string GetUid() => _environmentService.GetEnvironmentVariable("UID") ??
                                   _environmentService.GetEnvironmentVariable("KDE_SESSION_UID");
    }
}