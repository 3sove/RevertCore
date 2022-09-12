namespace Revert.Core.IO
{
    public interface ITextDocumentParser
    {
        string GetDocumentText(System.IO.FileInfo file);
    }
}
