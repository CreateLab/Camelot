﻿namespace Camelot.Services.Abstractions.Models
{
    public class ApplicationModel
    {
        public string FileExtension { get; set; }

        public string DisplayIcon { get; set; }

        public string DisplayName { get; set; }

        public string DisplayVersion { get; set; }

        public string InstallLocation { get; set; }

        public string StartCommand { get; set; }

        public string ExecutePath { get; set; }
    }
}