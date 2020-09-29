using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Deletes a file or folder from the file system.
    /// </summary>
    public class DeleteItemStepFactory : SimpleStepFactory<DeleteItem, Unit>
    {
        private DeleteItemStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DeleteItem, Unit> Instance { get; } = new DeleteItemStepFactory();
    }
}