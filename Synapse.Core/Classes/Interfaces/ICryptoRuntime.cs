﻿using System;

namespace Synapse.Core
{
    public interface ICryptoRuntime
    {
        string ActionName { get; set; }
        string RuntimeType { get; set; }

        object GetConfigInstance();
        object GetParametersInstance();

        ICryptoRuntime Initialize(string config);

        ExecuteResult Logon(CryptoStartInfo startInfo);
        void Logoff();

        event EventHandler<LogMessageEventArgs> LogMessage;
    }

    public class CryptoStartInfo : StartInfoBase
    {
        public CryptoStartInfo() { }

        /// <summary>
        /// Initialize this instance from the parameter instance.
        /// </summary>
        /// <param name="si">StartInfo instance used to initialise this instance.</param>
        public CryptoStartInfo(IStartInfo si)
        {
            RequestNumber = si.RequestNumber;
            RequestUser = si.RequestUser;
        }

        public bool IsDryRun { get; internal set; }

        public string Parameters { get; internal set; }
    }
}