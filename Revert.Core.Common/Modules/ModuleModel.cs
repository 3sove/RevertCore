using System;

namespace Revert.Core.Common.Modules
{
    public abstract class ModuleModel
    {
        /// <summary>
        /// Determines whether update messages should be allowed or squelched.
        /// </summary>
        public bool CanPublishUpdateMessages { get; set; } = true;

        /// <summary>
        /// Action which is performed to output update messages.  Defaults to Console.Writeline.
        /// </summary>
        public Action<string> UpdateMessageAction { set; get; } = Console.WriteLine;

        /// <summary>
        /// Determines how often a message is output by emitting a message once per n messages.  Defaults to 1000.
        /// </summary>
        public int RecordsPerMessage { get; set; } = 1000;
    }
}
