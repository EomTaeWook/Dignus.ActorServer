// Copyright (c) 2026 EomTaeWook
// Licensed under the MIT License. See LICENSE file in the project root.
// Part of Dignus.ActorServer

using Dignus.Framework.Pipeline;
using Dignus.Sockets;
using Dignus.Sockets.Interfaces;
using Dignus.Sockets.Pipeline;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dignus.Actor.Network
{
    public static class ActorProtocolPipeline<TPipelineContext> where TPipelineContext : struct, IPipelineContextBase
    {
        private static Func<int, bool> _validateProtocolDelegate;
        public static void Register<TProtocol>(Action<MethodInfo, AsyncPipeline<TPipelineContext>> setupMiddlewareAction)
            where TProtocol : struct, Enum
        {
            Type matchedPipelineContextInterface = null;

            foreach(var interfaceType in typeof(TPipelineContext).GetInterfaces())
            {
                if(interfaceType.IsGenericType == false)
                {
                    continue;
                }
                if(interfaceType.GetGenericTypeDefinition() == typeof(IPipelineContext<,,>))
                {
                    matchedPipelineContextInterface = interfaceType;
                    break;
                }
            }

            if (matchedPipelineContextInterface == null)
            {
                throw new InvalidOperationException($"{typeof(TPipelineContext).FullName} does not implement IPipelineContext<THandler, TBody, TState>.");
            }

            var genericArgumentTypes = matchedPipelineContextInterface.GetGenericArguments();
            var handlerType = genericArgumentTypes[0];
            var bodyType = genericArgumentTypes[1];
            var stateType = genericArgumentTypes[2];

            var binderType = typeof(ProtocolHandlerBinder<,,,>).MakeGenericType(
                typeof(TPipelineContext),
                handlerType,
                bodyType,
                stateType);

            var binderInstance = Activator.CreateInstance(binderType) ?? throw new InvalidOperationException($"failed to create binder instance: {binderType.FullName}");

            var registerInternalMethodInfo = typeof(ActorProtocolPipeline<TPipelineContext>).GetMethod(
                nameof(RegisterInternal),
                BindingFlags.NonPublic | BindingFlags.Static);

            var registerInternalMethod = registerInternalMethodInfo.MakeGenericMethod(handlerType, bodyType, stateType, typeof(TProtocol));

            registerInternalMethod.Invoke(obj: null, parameters: [setupMiddlewareAction, binderInstance]);

            var validateMethodInfo = typeof(ProtocolStateHandlerMapper)
                .GetMethod(nameof(ProtocolStateHandlerMapper.ValidateProtocol), BindingFlags.Public | BindingFlags.Static)
                ?.MakeGenericMethod(handlerType, stateType);

            _validateProtocolDelegate = (Func<int, bool>)Delegate.CreateDelegate(typeof(Func<int, bool>), validateMethodInfo);
        }
        private static void RegisterInternal<THandler, TBody, TState, TProtocol>(
            Action<MethodInfo, AsyncPipeline<TPipelineContext>> setupMiddlewareAction,
            IProtocolInvokerBinder<TPipelineContext> protocolInvokerBinder)
            where THandler : class, IProtocolHandler<TBody>
            where TProtocol : struct, Enum
        {
            ProtocolPipelineInvoker<TPipelineContext>
                .Bind<THandler, TBody, TProtocol>(protocolInvokerBinder)
                .Use(setupMiddlewareAction)
                .Build();
        }
        public static bool ValidateProtocol(int protocol)
        {
            if(_validateProtocolDelegate == null)
            {
                throw new InvalidOperationException("ActorProtocolPipeline is not initialized. call Register first.");
            }
            return _validateProtocolDelegate(protocol);
        }
        public static Task ExecuteAsync(ref TPipelineContext pipelineContext)
        {
            return ProtocolPipelineInvoker<TPipelineContext>.ExecuteAsync(ref pipelineContext);
        }
    }
}