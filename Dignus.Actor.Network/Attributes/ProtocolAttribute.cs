using System;

namespace Dignus.Actor.Network.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited =false)]

    public class ProtocolAttribute(int protocol) : Attribute
    {
        public int Protocol { get; } = protocol;
    }
}
