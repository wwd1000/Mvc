// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationNode
    {
        public ModelValidationNode([NotNull] string key, ModelExplorer explorer) : this (key, explorer, new List<ModelValidationNode>())
        {
        }

        public ModelValidationNode([NotNull] string key, ModelExplorer explorer, [NotNull] IList<ModelValidationNode> childNodes)
        {
            Key = key;
            ModelExplorer = explorer;
            ChildNodes = childNodes;
        }

        public string Key { get; set; }

        public ModelExplorer ModelExplorer { get; set; }

        public IList<ModelValidationNode> ChildNodes { get; set; }
    }
}