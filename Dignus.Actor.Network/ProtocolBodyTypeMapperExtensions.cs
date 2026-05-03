// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.

using Dignus.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dignus.Actor.Network
{
    public static class ProtocolBodyTypeMapperExtensions
    {
        public static void RegisterByProtocolName<TProtocol>(this ProtocolBodyTypeMapper protocolBodyTypeMapper, Assembly assembly) 
            where TProtocol : struct, Enum
        {
            var protocolNames = new UniqueSet<string>();
            protocolNames.AddRange(Enum.GetNames<TProtocol>());

            foreach (var type in assembly.GetTypes())
            {
                if (protocolNames.Contains(type.Name))
                {
                    var protocol = Enum.Parse<TProtocol>(type.Name);
                    protocolBodyTypeMapper.AddMapping(protocol, type);
                }
            }
        }
        public static void RegisterByProtocolName<TProtocol>(this ProtocolBodyTypeMapper protocolBodyTypeMapper, IEnumerable<Assembly> assemblies)
            where TProtocol : struct, Enum
        {
            foreach(var assembly in assemblies)
            {
                RegisterByProtocolName<TProtocol>(protocolBodyTypeMapper, assembly);
            }
        }
    }
}
