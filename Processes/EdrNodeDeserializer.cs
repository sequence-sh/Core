using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Utilities;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Special type of Node Deserializer for EDR
    /// Ignores any property called ignore (but still parses any anchors set on it)
    /// If it finds a dictionary called 'Defaults' it will use that dictionary to set the value of that property anywhere in the following yaml.
    /// </summary>
    internal sealed class EdrNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory _objectFactory = new DefaultObjectFactory();
        private readonly ITypeInspector _typeDescriptor;

        public EdrNodeDeserializer(ITypeInspector typeDescriptor)
        {
            _typeDescriptor = typeDescriptor;
        }

        private const string Ignore = "ignore";
        private const string Defaults = "defaults";

        private readonly Dictionary<string, string> _currentDefaults = new Dictionary<string, string>();


        /// <inheritdoc />
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? resultValue)
        {
            if (!YamlHelper.SpecialTypesSet.Value.Contains(expectedType))
            {
                resultValue = null;
                return false;
            }


            if (!parser.TryConsume<MappingStart>(out _))
            {
                resultValue = null;
                return false;
            }

            var lazyResultValue = new Lazy<object>(()=>_objectFactory.Create(expectedType));

            var setProperties = new HashSet<string>();

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var scalar = parser.Consume<Scalar>();
                if (string.Equals(scalar.Value, Ignore, StringComparison.OrdinalIgnoreCase))
                {
                    nestedObjectDeserializer(parser, typeof(object));
                }
                else if (string.Equals(scalar.Value, Defaults, StringComparison.OrdinalIgnoreCase))
                {
                   var dict = nestedObjectDeserializer(parser, typeof(Dictionary<string, string>));

                   if (dict is IDictionary<string, string> defaultsDict)
                       foreach (var (key, s) in defaultsDict)
                           _currentDefaults[key] = s;
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
                            valuePromise.ValueAvailable += v => property.Write(lazyResultValue.Value, TypeConverter.ChangeType(v, property.Type));
                        }
                        else
                        {
                            var obj2 = TypeConverter.ChangeType(obj1, property.Type);
                            property.Write(lazyResultValue.Value, obj2);
                        }

                        setProperties.Add(property.Name);
                    }
                }
            }

            resultValue = lazyResultValue.Value;

            var defaultsToSet = _currentDefaults.Where(x => !setProperties.Contains(x.Key)).ToList();

            if (defaultsToSet.Any())
            {
                foreach (var (key, s) in defaultsToSet)
                {
                    var property = _typeDescriptor.GetProperty(expectedType, null, key, true);

                    if (property != null)
                    {
                        var typedValue = TypeConverter.ChangeType(s, property.Type);
                        property.Write(lazyResultValue.Value, typedValue); 
                    }
                }
            }
            
            return true;
        }
    }
}
