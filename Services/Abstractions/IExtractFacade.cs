﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IExtractFacade
    {
        Task<Dictionary<string, string>> ExtractCodeFromArchive(Stream archiveStream);
    }
}
