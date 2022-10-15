using System;
using System.IO;
using System.Linq;
using System.Text;
using Revert.Core.Extensions;

namespace Revert.Core.IO.Files
{
    public class LargeFileSplitter
    {
        public LargeFileSplitterModel Model { get; }

        public LargeFileSplitter(LargeFileSplitterModel model)
        {
            Model = model;
        }

        protected void Execute()
        {
            foreach (var fileName in Model.FileNames)
            {
                var directoryName = fileName.Substring(0, fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                switch (Model.SplitOptions)
                {
                    case FileSplitOptions.SplitByByteCount:
                        SplitByByteCount(Model, fileName, directoryName);
                        break;
                    case FileSplitOptions.SplitByLineCount:
                        SplitByLineCount(Model, fileName, directoryName);
                        break;
                }
            }
        }

        public void SplitByByteCount(LargeFileSplitterModel model, string filePath, string directoryName)
        {
            var file = new FileInfo(filePath);

            var fileBytes = new byte[model.FileSplitSize];

            using (var reader = File.OpenRead(filePath))
            {
                reader.Read(fileBytes, 0, model.FileSplitSize);
                var printableCharacters =
                    Encoding.Default.GetString(fileBytes).Where(rsChar => !char.IsControl(rsChar)).ToArray();
                fileBytes = Encoding.Default.GetBytes(printableCharacters);

                var smallSamplePath = directoryName.AddFilePath("Small Sample\\");

                if (!Directory.Exists(smallSamplePath))
                    smallSamplePath.CreateDirectory();

                var targetFileName = $"{smallSamplePath.AddFilePath(file.Name)}.txt";
                using (var writer = File.OpenWrite(targetFileName))
                {
                    writer.Write(fileBytes, 0, fileBytes.Length);
                    writer.Flush();
                    writer.Close();
                }

                reader.Close();
            }
        }

        public void SplitByLineCount(LargeFileSplitterModel model, string filePath, string directoryName)
        {
            var fileText = new StringBuilder();

            var lineReader = new LineReader(filePath, true, 10000);

            var file = new FileInfo(filePath);

            var fileName = file.GetFileNameWithoutExtension();

            var linesRead = 0;
            var filesWritten = 1;

            foreach (var line in lineReader)
            {
                fileText.AppendLine(line);

                //if ((linesRead % 10000) == 1)
                //{
                //    WriteFile(string.Format("{0}Segment {1} of {2}.txt", directoryName, filesWritten, fileName), fileText.ToString());
                //    fileText.Clear();
                //}

                if (++linesRead == model.FileSplitSize)
                {
                    WriteFile(string.Format("{0}Segment {1} of {2}.txt", directoryName, filesWritten++, fileName), fileText.ToString());
                    fileText.Clear();
                    linesRead = 0;
                }
            }

            if (fileText.Length != 0) WriteFile(string.Format("{0}Segment {1} of {2}", directoryName, ++filesWritten, fileName), fileText.ToString());
        }

        public void WriteFile(string fileName, string text)
        {
            using (var writer = File.Exists(fileName) ? File.AppendText(fileName) : File.CreateText(fileName))
            {
                writer.Write(text);
                writer.Flush();
                writer.Close();
            }
        }
    }
}
