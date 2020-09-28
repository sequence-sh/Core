using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
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