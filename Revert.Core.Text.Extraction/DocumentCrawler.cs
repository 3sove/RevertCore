using System;
using System.IO;
using System.Linq;
using Revert.Core.Extensions;

namespace Revert.Core.Text.Extraction
{
    public class DirectoryCrawler
    {
        private DirectoryInfo rootDirectoryInfo;

        public string RootDirectory { get; set; }

        public DirectoryInfo RootDirectoryInfo
        {
            get { return rootDirectoryInfo ?? (rootDirectoryInfo = new DirectoryInfo(RootDirectory)); }
            private set { rootDirectoryInfo = value; }
        }

        public bool IncludeSubDirectories { get; set; }
        public Func<FileInfo, bool> FileConditionEvaluator { get; set; }
        public Action<FileInfo> OnFileMatched { get; set; }

        public DirectoryCrawler(string directory, bool includeSubdirectories, Func<FileInfo, bool> fileConditionEvaluator, Action<FileInfo> onFileMatched)
            : this(directory, includeSubdirectories, onFileMatched)
        {
            FileConditionEvaluator = fileConditionEvaluator;
        }

        public DirectoryCrawler(string directory, bool includeSubdirectories, Action<FileInfo> onFileMatched)
        {
            RootDirectory = directory;
            IncludeSubDirectories = includeSubdirectories;
            OnFileMatched = onFileMatched;
        }

        public bool Execute()
        {
            return Execute(RootDirectoryInfo);
        }

        public static bool Crawl(string directory, bool includeSubdirectories, Func<FileInfo, bool> fileConditionEvaluator, Action<FileInfo> onFileMatched)
        {
            var directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists) return false;

            if (includeSubdirectories)
            {
                try
                {
                    if (!directoryInfo.HasReadAccess()) return false;
                    directoryInfo.GetDirectories().ForEach(subDirectory => Execute(subDirectory, includeSubdirectories, fileConditionEvaluator, onFileMatched));
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            directoryInfo.GetFiles().ForEach(onFileMatched);
            return true;
        }

        public static bool Crawl(string directory, bool includeSubdirectories, Action<FileInfo> onFileMatched)
        {
            return Crawl(directory, includeSubdirectories, info => true, onFileMatched);
        }

        private static bool Execute(DirectoryInfo directory, bool includeSubdirectories, Func<FileInfo, bool> fileConditionEvaluator, Action<FileInfo> onFileMatched)
        {
            if (directory == null || !directory.Exists) return false;

            if (includeSubdirectories)
            {
                try
                {
                    if (!directory.HasReadAccess()) return false;
                    directory.GetDirectories().ForEach(subDirectory => Execute(subDirectory, includeSubdirectories, fileConditionEvaluator, onFileMatched));
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            directory.GetFiles().Where(file => fileConditionEvaluator == null || fileConditionEvaluator(file)).ForEach(onFileMatched);
            return true;
        }

        private bool Execute(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists) return false;

            if (IncludeSubDirectories)
            {
                try
                {
                    if (!directory.HasReadAccess()) return false;
                    directory.GetDirectories().ForEach(subDirectory => Execute(subDirectory));
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            directory.GetFiles().Where(file => FileConditionEvaluator == null || FileConditionEvaluator(file)).ForEach(OnFileMatched);
            return true;
        }
    }


}