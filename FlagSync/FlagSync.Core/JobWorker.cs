﻿using System;
using System.Collections.Generic;
using System.Threading;
using FlagLib.FileSystem;

namespace FlagSync.Core
{
    public sealed class JobWorker
    {
        /// <summary>
        /// Occurs when a file has been proceeded.
        /// </summary>
        public event EventHandler<FileProceededEventArgs> ProceededFile;

        /// <summary>
        /// Occurs before a file gets deleted.
        /// </summary>
        public event EventHandler<FileDeletionEventArgs> DeletingFile;

        /// <summary>
        /// Occurs when a file has been deleted.
        /// </summary>
        public event EventHandler<FileDeletionEventArgs> DeletedFile;

        /// <summary>
        /// Occurs before a new file gets created.
        /// </summary>
        public event EventHandler<FileCopyEventArgs> CreatingFile;

        /// <summary>
        /// Occurs when a new file has been created.
        /// </summary>
        public event EventHandler<FileCopyEventArgs> CreatedFile;

        /// <summary>
        /// Occurs before a file gets modified.
        /// </summary>
        public event EventHandler<FileCopyEventArgs> ModifyingFile;

        /// <summary>
        /// Occurs when a file has been modified.
        /// </summary>
        public event EventHandler<FileCopyEventArgs> ModifiedFile;

        /// <summary>
        /// Occurs before a new directory gets created.
        /// </summary>
        public event EventHandler<DirectoryCreationEventArgs> CreatingDirectory;

        /// <summary>
        /// Occurs when a new directory has been created.
        /// </summary>
        public event EventHandler<DirectoryCreationEventArgs> CreatedDirectory;

        /// <summary>
        /// Occurs before a directory has been deleted.
        /// </summary>
        public event EventHandler<DirectoryDeletionEventArgs> DeletingDirectory;

        /// <summary>
        /// Occurs when a directory has been deleted.
        /// </summary>
        public event EventHandler<DirectoryDeletionEventArgs> DeletedDirectory;

        /// <summary>
        /// Occurs when a file copy error has occured.
        /// </summary>
        public event EventHandler<FileCopyErrorEventArgs> FileCopyError;

        /// <summary>
        /// Occurs when a file deletion error has occured.
        /// </summary>
        public event EventHandler<FileDeletionErrorEventArgs> FileDeletionError;

        /// <summary>
        /// Occurs when a directory deletion error has been catched.
        /// </summary>
        public event EventHandler<DirectoryDeletionEventArgs> DirectoryDeletionError;

        /// <summary>
        /// Occurs when the file copy progress has changed.
        /// </summary>
        public event EventHandler<CopyProgressEventArgs> FileCopyProgressChanged;

        /// <summary>
        /// Occurs when the job worker has finished.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Occurs when the files had been counted.
        /// </summary>
        public event EventHandler FilesCounted;

        /// <summary>
        /// Occurs when a job has started.
        /// </summary>
        public event EventHandler<JobEventArgs> JobStarted;

        /// <summary>
        /// Occurs when a job has finished.
        /// </summary>
        public event EventHandler<JobEventArgs> JobFinished;

        private Job currentJob;
        private Queue<Job> jobQueue = new Queue<Job>();
        private long totalWrittenBytes;
        private int proceededFiles;
        private FileCounter.FileCounterResults fileCounterResult;

        /// <summary>
        /// Gets the total written bytes.
        /// </summary>
        /// <value>The total written bytes.</value>
        public long TotalWrittenBytes
        {
            get
            {
                return this.totalWrittenBytes;
            }
        }

        /// <summary>
        /// Gets the proceeded files.
        /// </summary>
        /// <value>The proceeded files.</value>
        public int ProceededFiles
        {
            get
            {
                return this.proceededFiles;
            }
        }

        /// <summary>
        /// Gets the file counter result.
        /// </summary>
        /// <value>The file counter result.</value>
        public FileCounter.FileCounterResults FileCounterResult
        {
            get
            {
                return this.fileCounterResult;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JobWorker"/> is paused.
        /// </summary>
        /// <value>true if paused; otherwise, false.</value>
        public bool IsPaused
        {
            get
            {
                return this.currentJob == null ? false : this.currentJob.IsPaused;
            }
        }

        /// <summary>
        /// Stops the job worker.
        /// </summary>
        public void Stop()
        {
            if (currentJob != null)
            {
                this.currentJob.Stop();
                this.jobQueue.Clear();
            }
        }

        /// <summary>
        /// Pauses the job worker.
        /// </summary>
        public void Pause()
        {
            if (currentJob != null)
            {
                this.currentJob.Pause();
            }
        }

        /// <summary>
        /// Continues the job worker.
        /// </summary>
        public void Continue()
        {
            if (currentJob != null)
            {
                this.currentJob.Continue();
            }
        }

        /// <summary>
        /// Does the next job.
        /// </summary>
        private void DoNextJob()
        {
            if (this.jobQueue.Count > 0)
            {
                this.currentJob = this.jobQueue.Dequeue();
                this.InitializeCurrentJobEvents();
                this.OnJobStarted(new JobEventArgs(this.currentJob.Settings));
                this.currentJob.Start();
            }

            else
            {
                this.OnFinished(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Starts the job worker.
        /// </summary>
        private void Start()
        {
            Logger.Instance.LogStatusMessage("Start counting files");

            this.fileCounterResult = this.GetFileCounterResults();

            if (this.FilesCounted != null)
            {
                this.FilesCounted.Invoke(this, new EventArgs());
            }

            Logger.Instance.LogStatusMessage("Finished counting files");

            this.DoNextJob();
        }

        /// <summary>
        /// Starts the specified jobs.
        /// </summary>
        /// <param name="jobs">The jobs.</param>
        /// <param name="preview">if set to true, a preview will be performed.</param>
        public void Start(IEnumerable<JobSetting> jobs, bool preview)
        {
            this.totalWrittenBytes = 0;

            this.QueueJobs(jobs, preview);

            ThreadPool.QueueUserWorkItem(new WaitCallback(callback => this.Start()));
        }

        /// <summary>
        /// Queues the jobs.
        /// </summary>
        /// <param name="jobs">The jobs.</param>
        /// <param name="preview">if set to true, a preview will be performed.</param>
        private void QueueJobs(IEnumerable<JobSetting> jobs, bool preview)
        {
            foreach (JobSetting job in jobs)
            {
                switch (job.SyncMode)
                {
                    case SyncMode.Backup:
                        this.jobQueue.Enqueue(new BackupJob(job, preview));
                        break;

                    case SyncMode.Synchronization:
                        this.jobQueue.Enqueue(new SyncJob(job, preview));
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the file counter results.
        /// </summary>
        /// <returns></returns>
        private FileCounter.FileCounterResults GetFileCounterResults()
        {
            FileCounter.FileCounterResults result = new FileCounter.FileCounterResults();

            FileCounter counter = new FileCounter();

            foreach (Job job in this.jobQueue)
            {
                result += counter.CountJobFiles(job.Settings);
            }

            return result;
        }

        /// <summary>
        /// Initializes the current job events.
        /// </summary>
        private void InitializeCurrentJobEvents()
        {
            this.currentJob.CreatedDirectory += new EventHandler<DirectoryCreationEventArgs>(currentJob_CreatedDirectory);
            this.currentJob.CreatedFile += new EventHandler<FileCopyEventArgs>(currentJob_CreatedFile);
            this.currentJob.CreatingDirectory += new EventHandler<DirectoryCreationEventArgs>(currentJob_CreatingDirectory);
            this.currentJob.CreatingFile += new EventHandler<FileCopyEventArgs>(currentJob_CreatingFile);
            this.currentJob.DeletedDirectory += new EventHandler<DirectoryDeletionEventArgs>(currentJob_DeletedDirectory);
            this.currentJob.DeletedFile += new EventHandler<FileDeletionEventArgs>(currentJob_DeletedFile);
            this.currentJob.DeletingDirectory += new EventHandler<DirectoryDeletionEventArgs>(currentJob_DeletingDirectory);
            this.currentJob.DeletingFile += new EventHandler<FileDeletionEventArgs>(currentJob_DeletingFile);
            this.currentJob.DirectoryDeletionError += new EventHandler<DirectoryDeletionEventArgs>(currentJob_DirectoryDeletionError);
            this.currentJob.FileCopyError += new EventHandler<FileCopyErrorEventArgs>(currentJob_FileCopyError);
            this.currentJob.FileCopyProgressChanged += new EventHandler<CopyProgressEventArgs>(currentJob_FileCopyProgressChanged);
            this.currentJob.FileDeletionError += new EventHandler<FileDeletionErrorEventArgs>(currentJob_FileDeletionError);
            this.currentJob.Finished += new EventHandler(currentJob_Finished);
            this.currentJob.ModifiedFile += new EventHandler<FileCopyEventArgs>(currentJob_ModifiedFile);
            this.currentJob.ModifyingFile += new EventHandler<FileCopyEventArgs>(currentJob_ModifyingFile);
            this.currentJob.ProceededFile += new EventHandler<FileProceededEventArgs>(currentJob_ProceededFile);
        }

        /// <summary>
        /// Handles the ProceededFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileProceededEventArgs"/> instance containing the event data.</param>
        private void currentJob_ProceededFile(object sender, FileProceededEventArgs e)
        {
            if (this.ProceededFile != null)
            {
                this.ProceededFile(this, e);
            }
        }

        /// <summary>
        /// Handles the ModifyingFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileCopyEventArgs"/> instance containing the event data.</param>
        private void currentJob_ModifyingFile(object sender, FileCopyEventArgs e)
        {
            if (this.ModifyingFile != null)
            {
                this.ModifyingFile(this, e);
            }
        }

        /// <summary>
        /// Handles the ModifiedFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileCopyEventArgs"/> instance containing the event data.</param>
        private void currentJob_ModifiedFile(object sender, FileCopyEventArgs e)
        {
            if (this.ModifiedFile != null)
            {
                this.ModifiedFile(this, e);
            }
        }

        /// <summary>
        /// Handles the FileDeletionError event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileDeletionErrorEventArgs"/> instance containing the event data.</param>
        private void currentJob_FileDeletionError(object sender, FileDeletionErrorEventArgs e)
        {
            if (this.FileDeletionError != null)
            {
                this.FileDeletionError(this, e);
            }
        }

        /// <summary>
        /// Handles the FileCopyProgressChanged event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagLib.FileSystem.CopyProgressEventArgs"/> instance containing the event data.</param>
        private void currentJob_FileCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            if (this.FileCopyProgressChanged != null)
            {
                this.FileCopyProgressChanged(this, e);
            }
        }

        /// <summary>
        /// Handles the FileCopyError event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileCopyErrorEventArgs"/> instance containing the event data.</param>
        private void currentJob_FileCopyError(object sender, FileCopyErrorEventArgs e)
        {
            if (this.FileCopyError != null)
            {
                this.FileCopyError(this, e);
            }
        }

        /// <summary>
        /// Handles the DirectoryDeletionError event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.DirectoryDeletionEventArgs"/> instance containing the event data.</param>
        private void currentJob_DirectoryDeletionError(object sender, DirectoryDeletionEventArgs e)
        {
            if (this.DirectoryDeletionError != null)
            {
                this.DirectoryDeletionError(this, e);
            }
        }

        /// <summary>
        /// Handles the DeletingFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileDeletionEventArgs"/> instance containing the event data.</param>
        private void currentJob_DeletingFile(object sender, FileDeletionEventArgs e)
        {
            if (this.DeletingFile != null)
            {
                this.DeletingFile(this, e);
            }
        }

        /// <summary>
        /// Handles the DeletingDirectory event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.DirectoryDeletionEventArgs"/> instance containing the event data.</param>
        private void currentJob_DeletingDirectory(object sender, DirectoryDeletionEventArgs e)
        {
            if (this.DeletingDirectory != null)
            {
                this.DeletingDirectory(this, e);
            }
        }

        /// <summary>
        /// Handles the DeletedFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileDeletionEventArgs"/> instance containing the event data.</param>
        private void currentJob_DeletedFile(object sender, FileDeletionEventArgs e)
        {
            if (this.DeletedFile != null)
            {
                this.DeletedFile(this, e);
            }
        }

        /// <summary>
        /// Handles the DeletedDirectory event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.DirectoryDeletionEventArgs"/> instance containing the event data.</param>
        private void currentJob_DeletedDirectory(object sender, DirectoryDeletionEventArgs e)
        {
            if (this.DeletedDirectory != null)
            {
                this.DeletedDirectory(this, e);
            }
        }

        /// <summary>
        /// Handles the CreatingFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileCopyEventArgs"/> instance containing the event data.</param>
        private void currentJob_CreatingFile(object sender, FileCopyEventArgs e)
        {
            if (this.CreatingFile != null)
            {
                this.CreatingFile(this, e);
            }
        }

        /// <summary>
        /// Handles the CreatingDirectory event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.DirectoryCreationEventArgs"/> instance containing the event data.</param>
        private void currentJob_CreatingDirectory(object sender, DirectoryCreationEventArgs e)
        {
            if (this.CreatingDirectory != null)
            {
                this.CreatingDirectory(this, e);
            }
        }

        /// <summary>
        /// Handles the CreatedFile event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.FileCopyEventArgs"/> instance containing the event data.</param>
        private void currentJob_CreatedFile(object sender, FileCopyEventArgs e)
        {
            if (this.CreatedFile != null)
            {
                this.CreatedFile(this, e);
            }
        }

        /// <summary>
        /// Handles the CreatedDirectory event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FlagSync.Core.DirectoryCreationEventArgs"/> instance containing the event data.</param>
        private void currentJob_CreatedDirectory(object sender, DirectoryCreationEventArgs e)
        {
            if (this.CreatedDirectory != null)
            {
                this.CreatedDirectory(this, e);
            }
        }

        /// <summary>
        /// Handles the Finished event of the currentJob control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void currentJob_Finished(object sender, EventArgs e)
        {
            Job job = (Job)sender;

            this.OnJobFinished(new JobEventArgs(job.Settings));

            this.totalWrittenBytes += job.WrittenBytes;

            this.DoNextJob();
        }

        /// <summary>
        /// Raises the <see cref="E:Finished"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnFinished(EventArgs e)
        {
            if (this.Finished != null)
            {
                this.Finished(this, e);
            }

            Logger.Instance.LogStatusMessage("Finished work");
        }

        /// <summary>
        /// Raises the <see cref="E:JobStarted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="FlagSync.Core.JobEventArgs"/> instance containing the event data.</param>
        private void OnJobStarted(JobEventArgs e)
        {
            if (this.JobStarted != null)
            {
                this.JobStarted(this, e);
            }

            Logger.Instance.LogStatusMessage("Started job: " + e.Job.Name);
        }

        /// <summary>
        /// Raises the <see cref="E:JobFinished"/> event.
        /// </summary>
        /// <param name="e">The <see cref="FlagSync.Core.JobEventArgs"/> instance containing the event data.</param>
        private void OnJobFinished(JobEventArgs e)
        {
            if (this.JobFinished != null)
            {
                this.JobFinished(this, e);
            }

            Logger.Instance.LogStatusMessage("Finished job: " + e.Job.Name);
        }
    }
}