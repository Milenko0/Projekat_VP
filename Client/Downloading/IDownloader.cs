﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Downloading
{
    public interface IDownloader
    {
        void Download(string fileNameTemplate);
    }
}
