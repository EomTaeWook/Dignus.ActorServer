using System;
using System.Collections.Generic;

namespace Dignus.Actor.Network
{
    public class ProtocolBodyTypeMapper
    {
        private readonly Dictionary<int, Type> _protocolBodyTypes = new();

        public void AddMapping<TBody>(Enum protocol)
        {
            AddMapping(protocol, typeof(TBody));
        }
        public void AddMapping(Enum protocol, Type bodyType)
        {
            if (bodyType == null)
            {
                throw new ArgumentNullException(nameof(bodyType));
            }
            var protocolValue = Convert.ToInt32(protocol);

            if (_protocolBodyTypes.TryGetValue(protocolValue, out Type _))
            {
                throw new InvalidOperationException($"Protocol duplication: {protocolValue}");
            }
            var value = Convert.ToInt32(protocol);
            _protocolBodyTypes.Add(value, bodyType);
        }
        public Type GetBodyType(int protocol)
        {
            if (_protocolBodyTypes.TryGetValue(protocol, out Type bodyType) == false)
            {
                throw new InvalidOperationException($"Body type is not registered. Protocol: {protocol}");
            }

            return bodyType;
        }

        public bool Contains(int protocol)
        {
            return _protocolBodyTypes.ContainsKey(protocol);
        }
    }
}
