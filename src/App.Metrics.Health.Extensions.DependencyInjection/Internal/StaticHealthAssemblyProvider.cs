// <copyright file="StaticHealthAssemblyProvider.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Reflection;

namespace App.Metrics.Health.Extensions.DependencyInjection.Internal
{
    /// <summary>
    ///     Fixed set of candidate assemblies.
    /// </summary>
    internal sealed class StaticHealthAssemblyProvider
    {
        /// <summary>
        ///     Gets the list of candidate assemblies.
        /// </summary>
        public IList<Assembly> CandidateAssemblies { get; } = new List<Assembly>();
    }
}