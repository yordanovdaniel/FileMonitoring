﻿namespace FileMonitoringApp.Settings.FileTransfer
{
    internal class FileTransferAuthSettings
    {
        public FileTransferGrantTypes GrantType { get; set; } = FileTransferGrantTypes.Password;

        public FileTransferPasswordAuthCredentials? PasswordCredentials { get; set; }
    }
}
