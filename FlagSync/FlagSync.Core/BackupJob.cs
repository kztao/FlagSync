﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;

namespace FlagSync.Core
{
    /// <summary>
    /// A backup-job performs a synchronization only from directory A to directory B, but can check on deleted files
    /// </summary>
    class BackupJob : Job
    {
        /// <summary>
        /// Creates a new backup-job
        /// </summary>
        /// <param name="settings">The job-settings</param>
        /// <param name="preview">True, if changes should get performed, otherwise false (if you want to see what will happen when you perform a backup)</param>
        public BackupJob(JobSettings settings, bool preview)
            : base(settings, preview)
        {
            
        }

        /// <summary>
        /// Copies new and modified files from directory A to directory B and finally checks for deletions
        /// </summary>
        public override void Start()
        {
            //Backup directoryA to directoryB and then check for deletions
            this.BackupDirectories(new DirectoryInfo(this.Settings.DirectoryA), new DirectoryInfo(this.Settings.DirectoryB), this.Preview);
            this.CheckDeletions(new DirectoryInfo(this.Settings.DirectoryB), new DirectoryInfo(this.Settings.DirectoryA), this.Preview);

            this.OnFinished();
        }

        /// <summary>
        /// Checks for deleted files in directory B, which aren't in directory A
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="target">The target directory</param>
        /// <param name="preview">True, if changes should get performed, otherwise false (if you want to see what will happen when you perform a backup)</param>
        private void CheckDeletions(DirectoryInfo source, DirectoryInfo target, bool preview)
        {
            if (this.Stopped)
            {
                return;
            }

            try
            {
                foreach (FileInfo file in source.GetFiles())
                {
                    this.OnFileProceeded(file);

                    if (!File.Exists(Path.Combine(target.FullName, file.Name)))
                    {
                        if(preview)
                        {
                            this.OnDeletedFile(file);
                        }

                        else
                        {
                            try
                            {
                                file.Delete();
                            }

                            catch (IOException)
                            {
                                Logger.Instance.LogError("IOException at file deletion: " + file.FullName);
                                this.OnFileDeletionError(file);
                            }

                            catch (SecurityException)
                            {
                                Logger.Instance.LogError("SecurityException at file deletion: " + file.FullName);
                                this.OnFileDeletionError(file);
                            }

                            catch (UnauthorizedAccessException)
                            {
                                Logger.Instance.LogError("UnauthorizedAccessException at file deletion: " + file.FullName);
                                this.OnFileDeletionError(file);
                            }
                        }
                    }
                }

                foreach (DirectoryInfo directory in source.GetDirectories())
                {
                    if (!Directory.Exists(Path.Combine(target.FullName, directory.Name)))
                    {
                        FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);

                        foreach (FileInfo file in files)
                        {
                            this.OnFileProceeded(file);
                        }

                        if(preview)
                        {
                            this.OnDeletedDirectory(directory);
                        }

                        else
                        {
                            try
                            {
                                directory.Delete(true);
                                this.OnDeletedDirectory(directory);
                            }

                            catch (IOException)
                            {
                                Logger.Instance.LogError("IOException at directory deletion: " + directory.FullName);
                                this.OnDirectoryDeletionError(directory);
                            }

                            catch (System.Security.SecurityException)
                            {
                                Logger.Instance.LogError("SecurityException at directory deletion: " + directory.FullName);
                                this.OnDirectoryDeletionError(directory);
                            }

                            catch (UnauthorizedAccessException)
                            {
                                Logger.Instance.LogError("UnauthorizedAccessException at directory deletion: " + directory.FullName);
                                this.OnDirectoryDeletionError(directory);
                            }
                        }
                    }

                    else
                    {
                        this.CheckDeletions(directory, new DirectoryInfo(Path.Combine(target.FullName, directory.Name)), preview);
                    }
                }
            }

            catch (System.UnauthorizedAccessException)
            {
                Logger.Instance.LogError("UnauthorizedAccessException at directory: " + source.FullName);
            }
        }
    }
}