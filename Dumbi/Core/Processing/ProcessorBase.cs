﻿namespace Dumbi.Core.Processing
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Delegate that handles completion of processing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ProcessingCompleteEventHandler(object sender, ProcessingCompleteEventArgs e);

    /// <summary>
    /// Processor base
    /// </summary>
    public abstract class ProcessorBase : IProcessorBase, IDisposable
    {
        /// <summary>
        /// The background worker
        /// </summary>
        protected readonly BackgroundWorker worker = new BackgroundWorker();

        /// <summary>
        /// Indicate whether this instance is disposed or not
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Handler for receiving notification when the processing is complete
        /// </summary>
        public event ProcessingCompleteEventHandler Complete;

        /// <summary>
        /// Initializes this instance
        /// </summary>
        public ProcessorBase()
        {
            this.worker.DoWork += this.WorkerSetup;
            this.worker.RunWorkerCompleted += this.WorkerRunComplete;
        }

        /// <summary>
        /// Processes in a synchronous manner
        /// </summary>
        public void Process()
        {
            this.OnProcess();
        }

        /// <summary>
        /// Processes in an asynchronous manner
        /// </summary>
        public void ProcessAsync()
        {
            this.worker.RunWorkerAsync();
        }

        /// <summary>
        /// Cancels processing
        /// </summary>
        public void CancelProcessing()
        {
            this.worker.CancelAsync();
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region protected members

        /// <summary>
        /// Custom method that contains the processing logic
        /// </summary>
        protected abstract void OnProcess();

        /// <summary>
        /// Method that notifies the subscribers when the processing is complete
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The process complete event args</param>
        protected void OnProcessComplete(object sender, ProcessingCompleteEventArgs e)
        {
            if (!e.Cancelled)
            {
                var handlers = this.Complete;

                if (handlers != null)
                {
                    handlers(this, e);
                }
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.worker.IsBusy)
                    {
                        this.worker.CancelAsync();
                    }

                    this.worker.DoWork -= this.WorkerSetup;
                    this.worker.RunWorkerCompleted -= this.WorkerRunComplete;

                    this.worker.Dispose();
                }

                this.disposed = true;
            }
        }

        #endregion

        #region private members

        /// <summary>
        /// The setup method that wiresup the delegate to the backgroundworker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WorkerSetup(object sender, DoWorkEventArgs args)
        {
            this.OnProcess();
        }

        private void WorkerRunComplete(object sender, RunWorkerCompletedEventArgs args)
        {
            this.OnProcessComplete(sender, new ProcessingCompleteEventArgs(args.Error, args.Cancelled));
        }

        #endregion
    }
}
