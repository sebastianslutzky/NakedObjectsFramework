// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Meta.Facet;
using NakedObjects.ParallelReflect.FacetFactory;

namespace NakedObjects.ParallelReflect.Test.FacetFactory {
    [TestClass]
    public class PresentationHintAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        private PresentationHintAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof(IPresentationHintFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        #region Nested type: Customer

        [PresentationHint("ahint")]
        private class Customer { }

        #endregion

        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new PresentationHintAnnotationFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private class Customer1 {
            [PresentationHint("ahint")]
// ReSharper disable UnusedMember.Local
            public string FirstName {
                get { return null; }
            }

            [PresentationHint("ahint")]
            public List<Customer3> Customers { get; set; }
        }

        private class Customer2 {
            [PresentationHint("ahint")]
// ReSharper disable UnusedParameter.Local
            public void SomeAction([PresentationHint("ahint")] string foo) { }
        }

        private class Customer3 {
            [PresentationHint("ahint")]
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        private class Customer4 {
            public void SomeAction([PresentationHint("ahint")] int foo) { }
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Properties));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Actions));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.ActionParameters));
        }

        [TestMethod]
        public void TestPresentationHintAnnotationNotIgnoredForNonStringsProperty() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            PropertyInfo property = FindProperty(typeof(Customer3), "NumberOfOrders");
            metamodel = facetFactory.Process(Reflector, property, MethodRemover, Specification, metamodel);
            Assert.IsNotNull(Specification.GetFacet(typeof(IPresentationHintFacet)));
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationNotIgnoredForPrimitiveOnActionParameter() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo method = FindMethod(typeof(Customer4), "SomeAction", new[] {typeof(int)});
            metamodel = facetFactory.ProcessParams(Reflector, method, 0, Specification, metamodel);
            Assert.IsNotNull(Specification.GetFacet(typeof(IPresentationHintFacet)));
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationPickedUpOnAction() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo method = FindMethod(typeof(Customer2), "SomeAction", new[] {typeof(string)});
            metamodel = facetFactory.Process(Reflector, method, MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IPresentationHintFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is PresentationHintFacet);
            var maskFacet = (PresentationHintFacet) facet;
            Assert.AreEqual("ahint", maskFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationPickedUpOnActionParameter() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo method = FindMethod(typeof(Customer2), "SomeAction", new[] {typeof(string)});
            metamodel = facetFactory.ProcessParams(Reflector, method, 0, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IPresentationHintFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is PresentationHintFacet);
            var maskFacet = (PresentationHintFacet) facet;
            Assert.AreEqual("ahint", maskFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationPickedUpOnClass() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            metamodel = facetFactory.Process(Reflector, typeof(Customer), MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IPresentationHintFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is PresentationHintFacet);
            var maskFacet = (PresentationHintFacet) facet;
            Assert.AreEqual("ahint", maskFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationPickedUpOnCollectionProperty() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            PropertyInfo property = FindProperty(typeof(Customer1), "Customers");
            metamodel = facetFactory.Process(Reflector, property, MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IPresentationHintFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is PresentationHintFacet);
            var maskFacet = (PresentationHintFacet) facet;
            Assert.AreEqual("ahint", maskFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestPresentationHintAnnotationPickedUpOnProperty() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            PropertyInfo property = FindProperty(typeof(Customer1), "FirstName");
            metamodel = facetFactory.Process(Reflector, property, MethodRemover, Specification, metamodel);
            IFacet facet = Specification.GetFacet(typeof(IPresentationHintFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is PresentationHintFacet);
            var maskFacet = (PresentationHintFacet) facet;
            Assert.AreEqual("ahint", maskFacet.Value);
            Assert.IsNotNull(metamodel);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedParameter.Local
}