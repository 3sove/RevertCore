using ProtoBuf;

namespace Revert.Core.IO
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
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
