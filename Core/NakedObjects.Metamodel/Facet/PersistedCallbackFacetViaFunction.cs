// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Reflection;
using System.Runtime.Serialization;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Meta.Facet {
    [Serializable]
    public sealed class PersistedCallbackFacetViaFunction : PersistedCallbackFacetAbstract, IImperativeFacet {
        private readonly MethodInfo method;

        public PersistedCallbackFacetViaFunction(MethodInfo method, ISpecification holder)
            : base(holder) {
            this.method = method;
        }

        #region IImperativeFacet Members

        public MethodInfo GetMethod() {
            return method;
        }

        public Func<object, object[], object> GetMethodDelegate() {
            return null;
        }

        #endregion

        public override void Invoke(INakedObjectAdapter nakedObjectAdapter, ISession session, ILifecycleManager lifecycleManager, IMetamodelManager metamodelManager) {
            method.Invoke(null, new[] {nakedObjectAdapter.Object});
        }

        public override object InvokeAndReturn(INakedObjectAdapter nakedObjectAdapter, ISession session, ILifecycleManager lifecycleManager, IMetamodelManager metamodelManager) {
            return method.Invoke(null, new[] {nakedObjectAdapter.Object});
        }

        protected override string ToStringValues() {
            return "method=" + method;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) { }
    }

    // Copyright (c) Naked Objects Group Ltd.
}