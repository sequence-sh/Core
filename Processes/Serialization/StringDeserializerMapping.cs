//using CSharpFunctionalExtensions;

//namespace Reductech.EDR.Processes
//{
//    /// <summary>
//    /// Deserializes a regex group into a string
//    /// </summary>
//    public class StringDeserializerMapping : IDeserializerMapping
//    {
//        public StringDeserializerMapping(string groupName, string propertyName)
//        {
//            GroupName = groupName;
//            PropertyName = propertyName;
//        }

//        /// <inheritdoc />
//        public string GroupName { get; }

//        /// <inheritdoc />
//        public string PropertyName { get; }

//        /// <inheritdoc />
//        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => new ProcessMember(new ConstantFreezableProcess(groupText));
//    }
//}