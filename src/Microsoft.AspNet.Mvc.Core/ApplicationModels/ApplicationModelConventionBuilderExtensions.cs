// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public static class ApplicationModelConventionBuilderExtensions
    {
        public static IControllerModelConventionBuilder ForController<T>(this IApplicationModelConventionBuilder builder)
        {
            return builder.ForControllers(c => c.ControllerType == typeof(T).GetTypeInfo());
        }

        public static IControllerModelConventionBuilder ForAllControllers(this IApplicationModelConventionBuilder builder)
        {
            return builder.ForControllers(null);
        }

        public static IControllerModelConventionBuilder ForControllersInNamespace(this IApplicationModelConventionBuilder builder, string @namespace)
        {
            return builder.ForControllers(c => c.ControllerType.Namespace == @namespace);
        }
    }
}