using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Utilities;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Node deserializer that ignores any 'Ignore' property.
    /// </summary>
    internal sealed class EdrNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory _objectFactory = new DefaultObjectFactory();
        private readonly ITypeInspector _typeDescriptor;

        public EdrNodeDeserializer(ITypeInspector typeDescriptor)
        {
            this._typeDescriptor = typeDescriptor;
        }

        /// <inheritdoc />
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (!parser.TryConsume<MappingStart>(out _))
            {
                value = null;
                return false;
            }
            value = _objectFactory.Create(expectedType);
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var scalar = parser.Consume<Scalar>();
                if (string.Equals(scalar.Value, "ignore", StringComparison.OrdinalIgnoreCase))
                {
                    nestedObjectDeserializer(parser, typeof(object));
                    //hope this works
                }
                else
                {
                    var property = _typeDescriptor.GetProperty(expectedType, null, scalar.Value, false);
                    if (property == null)
                    {
                        parser.SkipThisAndNestedEvents();
                    }
                    else
                    {
                        var obj1 = nestedObjectDeserializer(parser, property.Type);
                        if (obj1 is IValuePromise valuePromise)
                        {
                            var valueRef = value;
                            valuePromise.ValueAvailable += v => property.Write(valueRef, TypeConverter.ChangeType(v, property.Type));
                        }
                        else
                        {
                            var obj2 = TypeConverter.ChangeType(obj1, property.Type);
                            property.Write(value, obj2);
                        }
                    }
                }

                
            }
            return true;
        }
    }
}
