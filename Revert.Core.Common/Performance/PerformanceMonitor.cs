using System;

namespace Revert.Core.Common.Performance
{
    /// <summary>
    /// Used for tracking and outputing performance characteristics of jobs performed on Enumerable collections
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly ulong totalRecordCount;
        public string JobName { get; set; }
        public long Position { get; private set; }
        public int RecordsPerUpdate { get; set; }
        public DateTime StartDateTime { get; }
        private Action<string> updateAction;
        public Action<string> UpdateAction
        {
            get { return updateAction ?? Console.WriteLine; }
            set { updateAction = value; }
        }

        public ulong TotalRecordCount { get; set; }

        public PerformanceMonitor(string jobName, int recordsPerUpdate, int totalRecordCount) : this(jobName, recordsPerUpdate, (ulong)totalRecordCount, Console.WriteLine)
        {
        }

        public PerformanceMonitor(string jobName, int recordsPerUpdate, int totalRecordCount, Action<string> updateAction) : this(jobName, recordsPerUpdate, (ulong)totalRecordCount, updateAction)
        {
        }

        public PerformanceMonitor(string jobName, int recordsPerUpdate, ulong totalRecordCount) : this(jobName, recordsPerUpdate, totalRecordCount, Console.WriteLine)
        {
        }

        public PerformanceMonitor(string jobName, int recordsPerUpdate, ulong totalRecordCount, Action<string> updateAction) 
        {
            this.totalRecordCount = totalRecordCount;
            UpdateAction = updateAction;
            JobName = jobName;
            RecordsPerUpdate = recordsPerUpdate;
            StartDateTime = DateTime.Now;
            TotalRecordCount = totalRecordCount;
        }

        /// <summary>
        ///  Tick should be called once per record in an enumerable as the work on that record is being completed.
        /// </summary>
        /// <returns></returns>
        public long Tick()
        {
            ++Position;
            if (Position%RecordsPerUpdate == 0)
                UpdateAction($"{JobName} - Processing records " +
                             $"{Position.ToString("#,#")} to " +
                             $"{((Position + RecordsPerUpdate) <= (long) TotalRecordCount ? (Position + RecordsPerUpdate).ToString("#,#") : TotalRecordCount.ToString("#,#"))} of " +
                             $"{TotalRecordCount.ToString("#,#")} at " +
                             $"{(Position/DateTime.Now.Subtract(StartDateTime).TotalHours).ToString("#,#")}/hour");
            return Position;
        }
    }
}
