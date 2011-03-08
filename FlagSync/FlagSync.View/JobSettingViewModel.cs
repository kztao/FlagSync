﻿using System;
using System.ComponentModel;
using System.IO;
using FlagLib.Patterns;
using FlagSync.Core;

namespace FlagSync.View
{
    public class JobSettingViewModel : ViewModelBase<JobSettingViewModel>, IDataErrorInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is included for syncing.
        /// </summary>
        /// <value>true if this instance is included for syncing; otherwise, false.</value>
        public bool IsIncluded
        {
            get { return this.InternJobSetting.IsIncluded; }
            set
            {
                if (this.IsIncluded != value)
                {
                    this.InternJobSetting.IsIncluded = value;
                    this.OnPropertyChanged(view => view.IsIncluded);
                }
            }
        }

        /// <summary>
        /// Gets or sets the directory A.
        /// </summary>
        /// <value>The directory A.</value>
        public string DirectoryA
        {
            get { return this.InternJobSetting.DirectoryA; }
            set
            {
                if (this.DirectoryA != value)
                {
                    this.InternJobSetting.DirectoryA = value;
                    this.OnPropertyChanged(view => view.DirectoryA);
                }
            }
        }

        /// <summary>
        /// Gets or sets the directory B.
        /// </summary>
        /// <value>The directory B.</value>
        public string DirectoryB
        {
            get { return InternJobSetting.DirectoryB; }
            set
            {
                if (this.DirectoryB != value)
                {
                    this.InternJobSetting.DirectoryB = value;
                    this.OnPropertyChanged(view => view.DirectoryB);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return this.InternJobSetting.Name; }
            set
            {
                if (this.Name != value)
                {
                    this.InternJobSetting.Name = value;
                    this.OnPropertyChanged(view => view.Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the sync mode.
        /// </summary>
        /// <value>The sync mode.</value>
        public SyncMode SyncMode
        {
            get { return this.InternJobSetting.SyncMode; }
            set
            {
                if (this.SyncMode != value)
                {
                    this.InternJobSetting.SyncMode = value;
                    this.OnPropertyChanged(vm => vm.SyncMode);
                    this.OnPropertyChanged(vm => vm.SyncModeString);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string SyncModeString
        {
            get
            {
                string syncMode = string.Empty;

                switch (this.SyncMode)
                {
                    case Core.SyncMode.FtpBackup:
                        syncMode = Properties.Resources.FtpServerBackupString;
                        break;

                    case Core.SyncMode.FtpSynchronization:
                        syncMode = Properties.Resources.FtpServerSynchronizationString;
                        break;

                    case Core.SyncMode.LocalBackup:
                        syncMode = Properties.Resources.LocalBackupString;
                        break;

                    case Core.SyncMode.LocalSynchronization:
                        syncMode = Properties.Resources.LocalSynchronizationString;
                        break;
                }

                return syncMode;
            }
        }

        /// <summary>
        /// Gets or sets the FTP server address.
        /// </summary>
        /// <value>The FTP server address.</value>
        public string FtpAddress
        {
            get { return this.InternJobSetting.FtpAddress; }
            set
            {
                if (this.FtpAddress != value)
                {
                    this.InternJobSetting.FtpAddress = value;
                    this.OnPropertyChanged(vm => vm.FtpAddress);
                }
            }
        }

        /// <summary>
        /// Gets or sets the login name of the FTP server user.
        /// </summary>
        /// <value>The login name of the FTP user.</value>
        public string FtpUserName
        {
            get { return this.InternJobSetting.FtpUserName; }
            set
            {
                if (this.FtpUserName != value)
                {
                    this.InternJobSetting.FtpUserName = value;
                    this.OnPropertyChanged(vm => vm.FtpUserName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the FTP server password.
        /// </summary>
        /// <value>The FTP server password.</value>
        public string FtpPassword
        {
            get { return this.InternJobSetting.FtpPassword; }
            set
            {
                if (this.FtpPassword != value)
                {
                    this.InternJobSetting.FtpPassword = value;
                    this.OnPropertyChanged(vm => vm.FtpPassword);
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server address.
        /// </summary>
        /// <value>The proxy server address.</value>
        public string ProxyAddress
        {
            get { return this.InternJobSetting.ProxyAddress; }
            set
            {
                if (this.ProxyAddress != value)
                {
                    this.InternJobSetting.ProxyAddress = value;
                    this.OnPropertyChanged(vm => vm.ProxyAddress);
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server port.
        /// </summary>
        /// <value>The proxy server port.</value>
        public int ProxyPort
        {
            get { return this.InternJobSetting.ProxyPort; }
            set
            {
                if (this.ProxyPort != value)
                {
                    this.InternJobSetting.ProxyPort = value;
                    this.OnPropertyChanged(vm => vm.ProxyPort);
                }
            }
        }

        /// <summary>
        /// Gets or sets the user name for the proxy server.
        /// </summary>
        /// <value>The user name for the proxy server.</value>
        public string ProxyUserName
        {
            get { return this.InternJobSetting.ProxyUserName; }
            set
            {
                if (this.ProxyUserName != value)
                {
                    this.InternJobSetting.ProxyUserName = value;
                    this.OnPropertyChanged(vm => vm.ProxyUserName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the proxy server password.
        /// </summary>
        /// <value>The proxy server password.</value>
        public string ProxyPassword
        {
            get { return this.InternJobSetting.ProxyPassword; }
            set
            {
                if (this.ProxyPassword != value)
                {
                    this.InternJobSetting.ProxyPassword = value;
                    this.OnPropertyChanged(vm => vm.ProxyPassword);
                }
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <value>The error message</value>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified name.
        /// </summary>
        /// <value></value>
        public string this[string name]
        {
            get
            {
                string result = null;

                switch (this.SyncMode)
                {
                    case Core.SyncMode.LocalBackup:
                    case Core.SyncMode.LocalSynchronization:
                        if ((name == "DirectoryA" && !Directory.Exists(this.DirectoryA)) || (name == "DirectoryB" && !Directory.Exists(this.DirectoryB)))
                        {
                            result = Properties.Resources.DirectoryDoesntExistMessage;
                        }

                        break;
                }

                if (name == "Name" && this.Name == String.Empty)
                {
                    result = Properties.Resources.NameFieldCantBeEmptyMessage;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the intern job setting.
        /// </summary>
        /// <value>The intern job setting.</value>
        public JobSetting InternJobSetting { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobSettingViewModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public JobSettingViewModel(string name)
            : this(new JobSetting(name))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobSettingViewModel"/> class.
        /// </summary>
        /// <param name="internJobSetting">The intern job setting.</param>
        public JobSettingViewModel(JobSetting internJobSetting)
        {
            this.InternJobSetting = internJobSetting;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.InternJobSetting.ToString();
        }
    }
}