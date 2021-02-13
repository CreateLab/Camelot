using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Camelot.Services.Abstractions;
using Camelot.Services.Abstractions.Drives;
using Camelot.Services.Abstractions.Models;
using Camelot.Services.AllPlatforms;

namespace Camelot.Services.Mac
{
    public class MacUnmountedDriveService : UnmountedDriveServiceBase
    {
        public override void Mount(string drive)
        {
            throw new NotImplementedException();
        }

        protected override Task<IReadOnlyList<UnmountedDriveModel>> GetUnmountedDrivesAsync()
        {
            throw new NotImplementedException();
        }
    }
}