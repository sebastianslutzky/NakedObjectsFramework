// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.ParallelReflect.FacetFactory;

namespace NakedObjects.ParallelReflect.Test.FacetFactory {
    [TestClass]
    public class IteratorFilteringFacetFactoryTest : AbstractFacetFactoryTest {
        private IteratorFilteringFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new Type[] { }; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Properties));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Actions));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameters));
        }

        [TestMethod]
        public void TestRequestsRemoverToRemoveIteratorMethods() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo enumeratorMethod = FindMethod(typeof(Customer), "GetEnumerator");
            metamodel = facetFactory.Process(Reflector, typeof(Customer), MethodRemover, Specification, metamodel);
            AssertMethodRemoved(enumeratorMethod);
            Assert.IsNotNull(metamodel);
        }

        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new IteratorFilteringFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming
        // ReSharper disable AssignNullToNotNullAttribute
        private class Customer : IEnumerable {
            #region IEnumerable Members

            public IEnumerator GetEnumerator() {
                return null;
            }

            #endregion

            public void someAction() { }
        }

        private class Customer1 {
            public void someAction() { }
        }

        // ReSharper restore AssignNullToNotNullAttribute
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local
    }

    // Copyright (c) Naked Objects Group Ltd.
}