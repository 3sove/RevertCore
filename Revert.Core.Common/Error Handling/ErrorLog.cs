using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Revert.Core.Common.Error_Handling
{
    public static class ErrorLog
    {
        private static string folderLocation = string.Empty;
        public static string FolderLocation
        {
            get
            {
                if (folderLocation == string.Empty)
                    throw new Exception("Error log folder location has not been set.  Please set the location before attempting to write to the log.");

                return folderLocation;
            }
            set
            {
                folderLocation = value;
            }
        }

        private static DirectoryInfo baseDirectory;
        private static FileInfo todaysErrorLog;

        private static readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        public static ErrorLogResponse Write(string logPath, MethodBase method, Exception exception, string currentUser)
        {
            FolderLocation = logPath;
            if (!FolderLocation.EndsWith("\\")) FolderLocation += "\\";
            return Write(method, exception, currentUser);
        }

        public static ErrorLogResponse Write(MethodBase method, Exception exception, string currentUser)
        {
            if (exception is ThreadAbortException)
                return new ErrorLogResponse(ErrorLogResponseTypes.Success, "Thread Abort Exception was thrown.");

            if (rwLock.TryEnterWriteLock(-1) == false)
                return new ErrorLogResponse(ErrorLogResponseTypes.Failure, "The system could not acquire the necessary locks.");

            try
            {
                if (baseDirectory == null) todaysErrorLog = GetTodaysLog();

                using (StreamWriter sw = new StreamWriter(todaysErrorLog.FullName, true))
                {
                    sw.WriteLine("Error");
                    sw.WriteLine("DateTime: " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sw.WriteLine("Class: " + method.DeclaringType);
                    sw.WriteLine("Method: " + method.Name);
                    sw.WriteLine("Error Message: " + exception.GetBaseException().Message);
                    sw.WriteLine("Call Stack: " + exception.StackTrace);
                    sw.WriteLine("Current User: " + currentUser);
                    sw.WriteLine("-------------------------------------------------");
                    sw.Flush();
                }
                return new ErrorLogResponse(ErrorLogResponseTypes.Success, "Error was written successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorLogResponse(ErrorLogResponseTypes.Failure, ex.GetBaseException().Message);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static ErrorLogResponse Write(MethodBase method, Exception exception, string message, string currentUser, string type)
        {
            if (exception is ThreadAbortException)
                return new ErrorLogResponse(ErrorLogResponseTypes.Success, "Thread Abort Exception was thrown.");

            if (exception?.Message.StartsWith("System Load Completed") == true)
                return new ErrorLogResponse(ErrorLogResponseTypes.Success, exception.Message);

            if (rwLock.TryEnterWriteLock(-1) == false) return new ErrorLogResponse(ErrorLogResponseTypes.Failure, "The system could not acquire the necessary locks.");

            try
            {
                if (baseDirectory == null) GetTodaysLog();

                using (StreamWriter sw = new StreamWriter(todaysErrorLog.FullName, true))
                {
                    sw.WriteLine(type);
                    sw.WriteLine("DateTime: " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));

                    if (method != null)
                    {
                        if (method.DeclaringType != null) sw.WriteLine("Class: " + method.DeclaringType);
                        sw.WriteLine("Method: " + method.Name);
                    }

                    if (exception != null)
                    {
                        sw.WriteLine("Error Message: " + exception.GetBaseException().Message);
                        sw.WriteLine("Call Stack: " + exception.StackTrace);
                    }

                    if (!string.IsNullOrEmpty(message))
                        sw.WriteLine("Message: " + message);

                    if (!string.IsNullOrEmpty(currentUser))
                        sw.WriteLine("Current User: " + currentUser);

                    sw.WriteLine("-------------------------------------------------");
                    sw.Flush();
                }
                return new ErrorLogResponse(ErrorLogResponseTypes.Success, type + " was written successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorLogResponse(ErrorLogResponseTypes.Failure, ex.GetBaseException().Message);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public static ErrorLogResponse WriteWarning(string message, MethodBase method = null, string currentUser = null)
        {
            return Write(method, null, message, currentUser, "Warning");
        }

        public static ErrorLogResponse WriteInformational(string message, string currentUser = null)
        {
            return Write(null, null, message, currentUser, "Information");
        }

        public static void SetErrorLogFolderLocation(string folderPath)
        {
            if (rwLock.TryEnterWriteLock(-1) == false) return;
            try
            {
                FolderLocation = folderPath;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private static FileInfo GetTodaysLog()
        {
            baseDirectory = new DirectoryInfo(FolderLocation);

            lock (baseDirectory)
            {
                if (baseDirectory.Exists == false) baseDirectory.Create();

                var filePath = FolderLocation + DateTime.Now.ToString("dd MMM yyyy") + ".log";
                todaysErrorLog = new FileInfo(filePath);
                if (!todaysErrorLog.Exists)
                {
                    using (var fs = todaysErrorLog.Create())
                    {
                    }
                }
                return todaysErrorLog;
            }
        }
    }
}
