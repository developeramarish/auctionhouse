﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Command.Mediator
{
    public abstract class CommandMediator
    {
        private const string CommandsAssemblyName = "Core.Command";

        private readonly IImplProvider _implProvider;
        internal static Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>> PreHandleCommandAttributeStrategies;
        internal static Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>> PostHandleCommandAttributeStrategies;

        static CommandMediator()
        {
            LoadCommandAttributeStrategies(CommandsAssemblyName);
        }

        internal static void LoadCommandAttributeStrategies(string commandsAssemblyName)
        {
            PreHandleCommandAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>>();
            PostHandleCommandAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>>();
            var commandAttributes = Assembly.Load(commandsAssemblyName)
                .GetTypes()
                .Where(type => type.BaseType == typeof(ICommand))
                .Where(type => type.GetCustomAttributes(typeof(ICommandAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (ICommandAttribute[]) type.GetCustomAttributes(typeof(ICommandAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in commandAttributes)
            {
                PreHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PreHandleAttributeStrategy)
                        .AsEnumerable();
                PostHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PostHandleAttributeStrategy)
                        .AsEnumerable();
            }
        }

        internal static void InvokePostCommandAttributeStrategies(IImplProvider implProvider, ICommand commandBase)
        {
            if (PostHandleCommandAttributeStrategies.ContainsKey(commandBase.GetType()))
            {
                foreach (var strategy in PostHandleCommandAttributeStrategies[commandBase.GetType()])
                {
                    strategy?.Invoke(implProvider, commandBase);
                }
            }
        }

        protected CommandMediator(IImplProvider implProvider)
        {
            _implProvider = implProvider;
        }

        public virtual async Task<RequestStatus> Send<T>(T command) where T : ICommand
        {
            if (PreHandleCommandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in PreHandleCommandAttributeStrategies[command.GetType()])
                {
                    strategy?.Invoke(_implProvider, command);
                }
            }

            var (requestStatus, callPostActions) = await SendAppCommand(command);

            if (callPostActions)
            {
                InvokePostCommandAttributeStrategies(_implProvider, command);
            }

            return requestStatus;
        }

        protected abstract Task<(RequestStatus, bool)> SendAppCommand<T>(T command) where T : ICommand;

 
    }
}