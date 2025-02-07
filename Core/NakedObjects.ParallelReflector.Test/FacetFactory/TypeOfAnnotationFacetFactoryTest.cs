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
    public class TypeOfAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        private TypeOfAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof(ITypeOfFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        [TestMethod]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Properties));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Actions));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameters));
        }

        [TestMethod]
        public void TestTypeOfFacetInferredForActionWithArrayReturnType() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo actionMethod = FindMethod(typeof(Customer9), "SomeAction");
            metamodel = facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification, metamodel);
            IFacet typeOfFacet = Specification.GetFacet(typeof(ITypeOfFacet));
            Assert.IsNotNull(typeOfFacet);
            Assert.IsTrue(typeOfFacet is TypeOfFacetInferredFromArray);

            var elementTypeFacet = Specification.GetFacet<IElementTypeFacet>();
            Assert.IsNotNull(elementTypeFacet);
            Assert.IsTrue(elementTypeFacet is ElementTypeFacet);
            Assert.AreEqual(typeof(Order), elementTypeFacet.Value);
            AssertNoMethodsRemoved();
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestTypeOfFacetInferredForActionWithGenericCollectionReturnType() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            MethodInfo actionMethod = FindMethod(typeof(Customer3), "SomeAction");
            metamodel = facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification, metamodel);
            IFacet typeOfFacet = Specification.GetFacet(typeof(ITypeOfFacet));
            Assert.IsNotNull(typeOfFacet);
            Assert.IsTrue(typeOfFacet is TypeOfFacetInferredFromGenerics);

            var elementTypeFacet = Specification.GetFacet<IElementTypeFacet>();
            Assert.IsNotNull(elementTypeFacet);
            Assert.IsTrue(elementTypeFacet is ElementTypeFacet);
            Assert.AreEqual(typeof(Order), elementTypeFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestTypeOfFacetInferredForCollectionWithGenericCollectionReturnType() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            PropertyInfo property = FindProperty(typeof(Customer4), "Orders");
            metamodel = facetFactory.Process(Reflector, property, MethodRemover, Specification, metamodel);
            IFacet typeOfFacet = Specification.GetFacet(typeof(ITypeOfFacet));
            Assert.IsNotNull(typeOfFacet);
            Assert.IsTrue(typeOfFacet is TypeOfFacetInferredFromGenerics);

            var elementTypeFacet = Specification.GetFacet<IElementTypeFacet>();
            Assert.IsNotNull(elementTypeFacet);
            Assert.IsTrue(elementTypeFacet is ElementTypeFacet);
            Assert.AreEqual(typeof(Order), elementTypeFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        [TestMethod]
        public void TestTypeOfFacetIsInferredForCollectionFromOrderArray() {
            IImmutableDictionary<string, ITypeSpecBuilder> metamodel = new Dictionary<string, ITypeSpecBuilder>().ToImmutableDictionary();

            PropertyInfo property = FindProperty(typeof(Customer10), "Orders");
            metamodel = facetFactory.Process(Reflector, property, MethodRemover, Specification, metamodel);
            IFacet typeOfFacet = Specification.GetFacet(typeof(ITypeOfFacet));
            Assert.IsNotNull(typeOfFacet);
            Assert.IsTrue(typeOfFacet is TypeOfFacetInferredFromArray);

            var elementTypeFacet = Specification.GetFacet<IElementTypeFacet>();
            Assert.IsNotNull(elementTypeFacet);
            Assert.IsTrue(elementTypeFacet is ElementTypeFacet);
            Assert.AreEqual(typeof(Order), elementTypeFacet.Value);
            Assert.IsNotNull(metamodel);
        }

        #region Nested type: Customer10

        private class Customer10 {
// ReSharper disable once UnusedMember.Local
            public Order[] Orders {
                get { return null; }
            }
        }

        #endregion

        #region Nested type: Customer3

        private class Customer3 {
// ReSharper disable once UnusedMember.Local
            public IList<Order> SomeAction() {
                return null;
            }
        }

        #endregion

        #region Nested type: Customer4

        private class Customer4 {
// ReSharper disable once UnusedMember.Local
            public IList<Order> Orders {
                get { return null; }
            }
        }

        #endregion

        #region Nested type: Customer9

        private class Customer9 {
// ReSharper disable once UnusedMember.Local
            public Order[] SomeAction() {
                return null;
            }
        }

        #endregion

        #region Nested type: Order

        private class Order { }

        #endregion

        #region Setup/Teardown

        [TestInitialize]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new TypeOfAnnotationFacetFactory(0);
        }

        [TestCleanup]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion
    }

    // Copyright (c) Naked Objects Group Ltd.
}