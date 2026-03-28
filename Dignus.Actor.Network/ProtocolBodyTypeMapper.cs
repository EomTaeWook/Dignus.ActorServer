using Dignus.Actor.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dignus.Actor.Network
{
    public class ProtocolBodyTypeMapper
    {
        private readonly Dictionary<int, Type> _protocolBodyTypes = new();
        public void Register(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentException("assemblies is empty.", nameof(assemblies));
            }

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                Type[] packetTypes = assembly.GetTypes();

                foreach (Type packetType in packetTypes)
                {
                    ProtocolAttribute protocolAttribute = packetType.GetCustomAttribute<ProtocolAttribute>();
                    if (protocolAttribute == null)
                    {
                        continue;
                    }

                    if (_protocolBodyTypes.TryGetValue(protocolAttribute.Protocol, out Type registeredBodyType))
                    {
                        throw new InvalidOperationException($"Protocol duplication: {protocolAttribute.Protocol}");
                    }

                    _protocolBodyTypes.Add(protocolAttribute.Protocol, packetType);
                }
            }
        }
        public void Add<TBody>(Enum protocol)
        {
            Add(protocol, typeof(TBody));
        }
        public void Add(Enum protocol, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var protocolValue = Convert.ToInt32(protocol);

            if (_protocolBodyTypes.TryGetValue(protocolValue, out Type _))
            {
                throw new InvalidOperationException($"Protocol duplication: {protocolValue}");
            }
            var value = Convert.ToInt32(protocol);
            _protocolBodyTypes.Add(value, type);
        }
        public Type GetBodyType(int protocol)
        {
            if (_protocolBodyTypes.TryGetValue(protocol, out Type bodyType) == false)
            {
                throw new InvalidOperationException($"Body type is not registered. Protocol: {protocol}");
            }

            return bodyType;
        }

        public bool ValidateProtocol(int protocol)
        {
            return _protocolBodyTypes.ContainsKey(protocol);
        }
    }
}
