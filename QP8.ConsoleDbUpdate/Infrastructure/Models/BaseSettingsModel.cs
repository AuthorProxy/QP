﻿using System.Collections.Generic;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models
{
    internal class BaseSettingsModel
    {
        internal readonly IList<string> FilePathes;
        internal readonly string CustomerCode;
        internal readonly string ConfigPath;
        internal readonly int UserId;

        public BaseSettingsModel(IList<string> pathes, string customerCode, string configPath)
        {
            FilePathes = pathes;
            CustomerCode = customerCode;
            ConfigPath = configPath;
            UserId = 1;
        }
    }
}