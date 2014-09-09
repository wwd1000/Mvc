// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public static class ApplicationModelCollectionExtensions
    {
        public static IApplicationModelConventionBuilder BuildConvention(this IList<IApplicationModelConvention> conventions)
        {
            var convention = new ApplicationModelConvention();
            conventions.Add(convention);

            return new ApplicationModelConventionBuilder(convention);
        }

        private class ApplicationModelConvention : IApplicationModelConvention
        {
            public IList<IControllerModelConvention> ControllerModelConventions { get; } = new List<IControllerModelConvention>();

            public IList<IApplicationModelConvention> Conventions { get; } = new List<IApplicationModelConvention>();

            public void Apply(ApplicationModel application)
            {
                foreach (var convention in Conventions)
                {
                    convention.Apply(application);
                }

                foreach (var convention in ControllerModelConventions)
                {
                    foreach (var controller in application.Controllers)
                    {
                        convention.Apply(controller);
                    }
                }
            }
        }

        private class ApplicationModelConventionBuilder : IApplicationModelConventionBuilder
        {
            public ApplicationModelConventionBuilder(ApplicationModelConvention convention)
            {
                Convention = convention;
            }

            private ApplicationModelConvention Convention { get; set; }

            public IApplicationModelConventionBuilder Add(IApplicationModelConvention convention)
            {
                Convention.Conventions.Add(convention);
                return this;
            }

            public IControllerModelConventionBuilder ForControllers(Func<ControllerModel, bool> predicate)
            {
                var controllerConvention = new ControllerModelConvention(predicate);
                Convention.ControllerModelConventions.Add(controllerConvention);
                return new ControllerModelConventionBuilder(controllerConvention);
            }
        }

        private class ControllerModelConvention : IControllerModelConvention
        {
            public ControllerModelConvention(Func<ControllerModel, bool> predicate)
            {
                Predicate = predicate;
            }

            public IList<IActionModelConvention> ActionModelConventions { get; } = new List<IActionModelConvention>();

            public IList<IControllerModelConvention> Conventions { get; } = new List<IControllerModelConvention>();

            private Func<ControllerModel, bool> Predicate { get; set; }

            public void Apply(ControllerModel controller)
            {
                if (Predicate == null || Predicate(controller))
                {
                    foreach (var convention in Conventions)
                    {
                        convention.Apply(controller);
                    }

                    foreach (var convention in ActionModelConventions)
                    {
                        foreach (var action in controller.Actions)
                        {
                            convention.Apply(action);
                        }
                    }
                }
            }
        }

        private class ControllerModelConventionBuilder : IControllerModelConventionBuilder
        {
            public ControllerModelConventionBuilder(ControllerModelConvention convention)
            {
                Convention = convention;
            }

            private ControllerModelConvention Convention { get; set; }

            public IControllerModelConventionBuilder Add(IControllerModelConvention convention)
            {
                Convention.Conventions.Add(convention);
                return this;
            }

            public IActionModelConventionBuilder ForActions(Func<ActionModel, bool> predicate)
            {
                var actionConvention = new ActionModelConvention(predicate);
                Convention.ActionModelConventions.Add(actionConvention);
                return new ActionModelConventionBuilder(actionConvention);
            }
        }

        private class ActionModelConvention : IActionModelConvention
        {
            public ActionModelConvention(Func<ActionModel, bool> predicate)
            {
                Predicate = predicate;
            }

            public IList<IActionModelConvention> Conventions { get; } = new List<IActionModelConvention>();

            private Func<ActionModel, bool> Predicate { get; set; }

            public void Apply(ActionModel action)
            {
                if (Predicate == null || Predicate(action))
                {
                    foreach (var convention in Conventions)
                    {
                        convention.Apply(action);
                    }
                }
            }
        }

        private class ActionModelConventionBuilder : IActionModelConventionBuilder
        {
            public ActionModelConventionBuilder(ActionModelConvention convention)
            {
                Convention = convention;
            }

            private ActionModelConvention Convention { get; set; }

            public IActionModelConventionBuilder Add(IActionModelConvention convention)
            {
                Convention.Conventions.Add(convention);
                return this;
            }
        }
    }
}