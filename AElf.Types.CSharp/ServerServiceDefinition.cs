#region Copyright notice and license

// Copyright 2015 gRPC authors. Modified by AElfProject.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using Google.Protobuf.Reflection;

namespace AElf.Types.CSharp
{
    /// <summary>
    /// Stores mapping of methods to server call handlers.
    /// Normally, the <c>ServerServiceDefinition</c> objects will be created by the <c>BindService</c> factory method 
    /// that is part of the autogenerated code for a protocol buffers service definition.
    /// </summary>
    public class ServerServiceDefinition
    {
        readonly IReadOnlyList<Action<ServiceBinderBase>> addMethodActions;
        readonly IReadOnlyList<Action<ServiceBinderBase>> addDescriptorActions;

        internal ServerServiceDefinition(List<Action<ServiceBinderBase>> addMethodActions, List<Action<ServiceBinderBase>> addDescriptorActions)
        {
            this.addMethodActions = addMethodActions.AsReadOnly();
            this.addDescriptorActions = addDescriptorActions.AsReadOnly();
        }

        /// <summary>
        /// Forwards all the previously stored <c>AddMethod</c> calls to the service binder.
        /// </summary>
        public void BindService(ServiceBinderBase serviceBinder)
        {
            foreach (var addMethodAction in addMethodActions)
            {
                addMethodAction(serviceBinder);
            }

            foreach (var addDescriptorAction in addDescriptorActions)
            {
                addDescriptorAction(serviceBinder);
            }
        }

        /// <summary>
        /// Creates a new builder object for <c>ServerServiceDefinition</c>.
        /// </summary>
        /// <returns>The builder object.</returns>
        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Builder class for <see cref="ServerServiceDefinition"/>.
        /// </summary>
        public class Builder
        {
            // to maintain legacy behavior, we need to detect duplicate keys and throw the same exception as before
            readonly Dictionary<string, object> duplicateDetector = new Dictionary<string, object>();
            // for each AddMethod call, we store an action that will later register the method and handler with ServiceBinderBase
            readonly List<Action<ServiceBinderBase>> addMethodActions = new List<Action<ServiceBinderBase>>();
            readonly List<Action<ServiceBinderBase>> addDescriptorActions = new List<Action<ServiceBinderBase>>();

            /// <summary>
            /// Creates a new instance of builder.
            /// </summary>
            public Builder()
            {
            }

            /// <summary>
            /// Adds a definition for a single request - single response method.
            /// </summary>
            /// <typeparam name="TRequest">The request message class.</typeparam>
            /// <typeparam name="TResponse">The response message class.</typeparam>
            /// <param name="method">The method.</param>
            /// <param name="handler">The method handler.</param>
            /// <returns>This builder instance.</returns>
            public Builder AddMethod<TRequest, TResponse>(
                Method<TRequest, TResponse> method,
                UnaryServerMethod<TRequest, TResponse> handler)
                    where TRequest : class
                    where TResponse : class
            {
                duplicateDetector.Add(method.FullName, null);
                addMethodActions.Add((serviceBinder) => serviceBinder.AddMethod(method, handler));
                return this;
            }

            public Builder AddDescriptors(IEnumerable<ServiceDescriptor> descriptors)
            {
                foreach (var descriptor in descriptors)
                {
                    addDescriptorActions.Add((serviceBinder) => serviceBinder.AddDescriptor(descriptor));
                }
                return this;
            }

            /// <summary>
            /// Creates an immutable <c>ServerServiceDefinition</c> from this builder.
            /// </summary>
            /// <returns>The <c>ServerServiceDefinition</c> object.</returns>
            public ServerServiceDefinition Build()
            {
                return new ServerServiceDefinition(addMethodActions, addDescriptorActions);
            }
        }
    }
}
