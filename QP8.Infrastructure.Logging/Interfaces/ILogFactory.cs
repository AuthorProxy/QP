﻿using System;

namespace QP8.Infrastructure.Logging.Interfaces
{
    /// <summary>
    /// Factory to create ILog instances.
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILog GetLogger();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        ILog GetLogger(Type type);

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        ILog GetLogger(string typeName);
    }
}