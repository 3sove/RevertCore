namespace Revert.Core.IO
{
    public interface ITextDocument
    {
        string Name { get; }

        /// <summary>
        /// Directory in the case of an NTFS document, or another meaningful category otherwise
        /// </summary>
        string DirectoryName { get; }
        string Text { get; }
    }
}
