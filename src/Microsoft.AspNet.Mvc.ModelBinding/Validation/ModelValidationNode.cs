// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelValidationNode
    {
        private readonly List<ModelValidationNode> _childNodes;

        public ModelValidationNode([NotNull] ModelExplorer modelExplorer, [NotNull] string modelStateKey)
            : this(modelExplorer, modelStateKey, childNodes: null)
        {
        }

        public ModelValidationNode(
            [NotNull] ModelExplorer modelExplorer,
            [NotNull] string modelStateKey,
            IEnumerable<ModelValidationNode> childNodes)
        {
            ModelExplorer = modelExplorer;
            ModelStateKey = modelStateKey;
            _childNodes = (childNodes != null) ? childNodes.ToList() : new List<ModelValidationNode>();
        }

        public ICollection<ModelValidationNode> ChildNodes
        {
            get
            {
                return _childNodes;
            }
        }

        public ModelExplorer ModelExplorer { get; }

        public string ModelStateKey { get; }

        public bool ValidateAllProperties { get; set; }

        public bool SuppressValidation { get; set; }

        /*
        // Reviewers: Removed `CombineWith()` because it looked incorrect when removed in f19c2e4 and wasn't used at
        // that time. For example, it should handle `this.SuppressValidation` and perhaps merge `ValidateAllProperties`
        // values as well.
        public void CombineWith(ModelValidationNode otherNode)
        {
            if (otherNode != null && !otherNode.SuppressValidation)
            {
                var otherChildNodes = otherNode._childNodes;
                for (var i = 0; i < otherChildNodes.Count; i++)
                {
                    var childNode = otherChildNodes[i];
                    _childNodes.Add(childNode);
                }
            }
        }
        */

        /*
        private object TryConvertContainerToMetadataType(ModelValidationNode parentNode)
        {
            if (parentNode != null)
            {
                var containerInstance = parentNode.ModelMetadata.Model;
                if (containerInstance != null)
                {
                    var expectedContainerType = ModelMetadata.ContainerType;
                    if (expectedContainerType != null)
                    {
                        if (expectedContainerType.IsCompatibleWith(containerInstance))
                        {
                            return containerInstance;
                        }
                    }
                }
            }

            return null;
        }

        public void Validate(ModelValidationContext validationContext)
        {
            Validate(validationContext, parentNode: null);
        }

        public void Validate([NotNull] ModelValidationContext validationContext, ModelValidationNode parentNode)
        {
            if (SuppressValidation || validationContext.ModelState.HasReachedMaxErrors)
            {
                // Short circuit if validation does not need to be applied or if we've reached the max number of
                // validation errors.
                return;
            }

            ValidateChildren(validationContext);
            ValidateThis(validationContext, parentNode);

            var modelState = validationContext.ModelState;
            if (modelState.GetFieldValidationState(ModelStateKey) != ModelValidationState.Invalid)
            {
                // If a node or its subtree were not marked invalid, we can consider it valid at this point.
                modelState.MarkFieldValid(ModelStateKey);
            }
        }

        private void ValidateChildren(ModelValidationContext validationContext)
        {
            for (var i = 0; i < _childNodes.Count; i++)
            {
                var child = _childNodes[i];
                ModelValidationContext childValidationContext = null; // **** new ModelValidationContext(validationContext, child.ModelMetadata);
                child.Validate(childValidationContext, this);
            }

            if (ValidateAllProperties)
            {
                ValidateProperties(validationContext);
            }
        }

        private void ValidateProperties(ModelValidationContext validationContext)
        {
            var modelState = validationContext.ModelState;

            var model = ModelMetadata.Model;
            var updatedMetadata = validationContext.MetadataProvider.GetMetadataForType(() => model,
                                                                                        ModelMetadata.ModelType);

            foreach (var propertyMetadata in updatedMetadata.Properties)
            {
                // Only want to add errors to ModelState if something doesn't already exist for the property node,
                // else we could end up with duplicate or irrelevant error messages.
                var propertyKeyRoot = ModelBindingHelper.CreatePropertyModelName(ModelStateKey,
                                                                                 propertyMetadata.PropertyName);

                if (modelState.GetFieldValidationState(propertyKeyRoot) == ModelValidationState.Unvalidated)
                {
                    var propertyValidators = GetValidators(validationContext, propertyMetadata);
                    var propertyValidationContext = new ModelValidationContext(validationContext, propertyMetadata);
                    foreach (var propertyValidator in propertyValidators)
                    {
                        foreach (var propertyResult in propertyValidator.Validate(propertyValidationContext))
                        {
                            var thisErrorKey = ModelBindingHelper.CreatePropertyModelName(propertyKeyRoot,
                                                                                          propertyResult.MemberName);
                            modelState.TryAddModelError(thisErrorKey, propertyResult.Message);
                        }
                    }
                }
            }
        }

        private void ValidateThis(ModelValidationContext validationContext, ModelValidationNode parentNode)
        {
            var modelState = validationContext.ModelState;
            if (modelState.GetFieldValidationState(ModelStateKey) == ModelValidationState.Invalid)
            {
                // If any item in the key's subtree has been identified as invalid, short-circuit
                return;
            }

            // If the Model at the current node is null and there is no parent, we cannot validate, and the
            // DataAnnotationsModelValidator will throw. So we intercept here to provide a catch-all value-required
            // validation error
            if (parentNode == null && ModelMetadata.Model == null)
            {
                modelState.TryAddModelError(ModelStateKey, Resources.Validation_ValueNotFound);
                return;
            }

            var container = TryConvertContainerToMetadataType(parentNode);
            var validators = GetValidators(validationContext, ModelMetadata).ToArray();
            for (var i = 0; i < validators.Length; i++)
            {
                var validator = validators[i];
                foreach (var validationResult in validator.Validate(validationContext))
                {
                    var currentModelStateKey = ModelBindingHelper.CreatePropertyModelName(ModelStateKey,
                                                                                          validationResult.MemberName);
                    modelState.TryAddModelError(currentModelStateKey, validationResult.Message);
                }
            }
        }

        private static IEnumerable<IModelValidator> GetValidators(ModelValidationContext validationContext,
                                                                  ModelMetadata metadata)
        {
            return validationContext.ValidatorProvider.GetValidators(metadata);
        }
        */
    }
}
