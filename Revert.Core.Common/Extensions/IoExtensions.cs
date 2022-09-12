using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Revert.Core.Extensions
{
    public static class IoExtensions
    {
        public static DirectoryInfo GetDirectory(this string directoryPath)
        {
            if (!CreateDirectory(directoryPath)) return null;
            return new DirectoryInfo(directoryPath);
        }

        public static bool CreateDirectory(this string directoryPath)
        {
            // *** Add Access Rule to the actual directory itself
            var info = new DirectoryInfo(directoryPath);
            if (!info.Exists) info.Create();
            return true;
        }

        private static SHA256 sha256Hasher;
        public static SHA256 Hasher => sha256Hasher ?? (sha256Hasher = new SHA256CryptoServiceProvider());

        public static string GetCleanFileName(this string fileName)
        {
            string result = fileName ?? "";
            return Path.GetInvalidFileNameChars().Aggregate(result, (current, nameChar) => current.Replace(nameChar.ToString(), string.Empty));
        }

        public static byte[] ComputeHash(this byte[] data, bool useSha256 = true)
        {
            if (useSha256) return Hasher.ComputeHash(data);
            unchecked
            {
                const int p = 16777619;
                var hash = data.Aggregate((int)2166136261, (current, t) => (current ^ t) * p);

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return BitConverter.GetBytes(hash);
            }
        }

        public static byte[] ComputeHash(this Stream stream)
        {
            return Hasher.ComputeHash(stream);
        }

        public static string ReadText(this string filePath)
        {
            var file = new FileInfo(filePath);
            using (var sr = file.OpenText())
                return sr.ReadToEnd();
        }

        public static string ReadText(this FileInfo file)
        {
            using (var sr = file.OpenText())
                return sr.ReadToEnd();
        }

        public static string GetNewFileName(this DirectoryInfo directory, string fileName)
        {
            var directoryFiles = directory.GetFiles();
            var info = new FileInfo(directory.FullName + fileName);
            var nameWithoutExtension = info.Name.GetFileNameWithoutExtension();
            var extension = info.Extension;
            var i = 0;
            while (directoryFiles.Any(f => f.Name == fileName))
            {
                fileName = $"{nameWithoutExtension} ({++i}){extension}";
            }

            var invalids = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            return $"{directory.FullName}{(directory.FullName.EndsWith("\\") ? string.Empty : "\\")}{fileName}";
        }

        public static string GetNewDirectoryName(this DirectoryInfo directory, string subDirectoryName)
        {
            var subDirectories = directory.GetDirectories();
            var i = 0;
            while (subDirectories.Any(f => f.Name == subDirectoryName))
                subDirectoryName = $"{subDirectoryName} ({++i})";
            return directory.FullName + subDirectoryName;
        }

        public static string GetFileNameWithoutExtension(this FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
        }
        
        public static bool HasReadAccess(this DirectoryInfo directory)
        {
            var readAllow = false;
            var readDeny = false;
            var security = new FileSecurity(directory.FullName, AccessControlSections.Access);
            var accessRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules)
            {
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Allow:
                        readAllow = true;
                        break;
                    case AccessControlType.Deny:
                        readDeny = true;
                        break;
                }
            }

            return readAllow && !readDeny;
        }

        public static bool HasWriteAccess(this DirectoryInfo directory)
        {
            var writeAllow = false;
            var writeDeny = false;
            var security = new FileSecurity(directory.FullName, AccessControlSections.Access);
            var accessRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if (!rule.FileSystemRights.HasFlag(FileSystemRights.Write)) continue;
                switch (rule.AccessControlType)
                {
                    case AccessControlType.Allow:
                        writeAllow = true;
                        break;
                    case AccessControlType.Deny:
                        writeDeny = true;
                        break;
                }
            }

            return writeAllow && !writeDeny;
        }


    }

}