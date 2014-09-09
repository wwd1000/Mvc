// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public static class ControllerModelConventionBuilderExtensions
    {
        public static IActionModelConventionBuilder ForAllActions(this IControllerModelConventionBuilder builder)
        {
            return builder.ForActions(null);
        }

        public static IActionModelConventionBuilder ForActionsNamed<T>(this IControllerModelConventionBuilder builder, string name)
        {
            return builder.ForActions(a => a.ActionName == name);
        }

        public static IControllerModelConventionBuilder DisableApiExplorer(this IControllerModelConventionBuilder builder)
        {
            // TODO
            return builder;
        }

        public static IControllerModelConventionBuilder EnableApiExplorer(this IControllerModelConventionBuilder builder)
        {
            // TODO
            return builder;
        }

        public static IControllerModelConventionBuilder AddRoute(this IControllerModelConventionBuilder builder, string route)
        {
            // TODO
            return builder;
        }
    }
}