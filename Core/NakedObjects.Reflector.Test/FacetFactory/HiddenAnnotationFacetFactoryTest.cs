// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Meta.Facet;
using NakedObjects.Reflect.FacetFactory;
using NUnit.Framework;

namespace NakedObjects.Reflect.Test.FacetFactory {
    [TestFixture]
    public class HiddenAnnotationFacetFactoryTest : AbstractFacetFactoryTest {
        #region Setup/Teardown

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            facetFactory = new HiddenAnnotationFacetFactory(0);
        }

        [TearDown]
        public override void TearDown() {
            facetFactory = null;
            base.TearDown();
        }

        #endregion

        private HiddenAnnotationFacetFactory facetFactory;

        protected override Type[] SupportedTypes {
            get { return new[] {typeof (IHiddenFacet)}; }
        }

        protected override IFacetFactory FacetFactory {
            get { return facetFactory; }
        }

        private class Customer {
            [Hidden]
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        private class Customer1 {
            [Hidden]
            public IList Orders {
                get { return null; }
            }
        }

        private class Customer2 {
            [Hidden]
            public void SomeAction() {}
        }

        private class Customer3 {
            [Hidden(WhenTo.Always)]
            public void SomeAction() {}
        }

        private class Customer4 {
            [Hidden(WhenTo.Never)]
            public void SomeAction() {}
        }

        private class Customer5 {
            [Hidden(WhenTo.OncePersisted)]
            public void SomeAction() {}
        }

        private class Customer6 {
            [Hidden(WhenTo.UntilPersisted)]
            public void SomeAction() {}
        }

        private class Customer7 {
            [ScaffoldColumn(false)]
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        private class Customer8 {
            [ScaffoldColumn(false)]
            public IList Orders {
                get { return null; }
            }
        }

        private class Customer9 {
            [ScaffoldColumn(true)]
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        private class Customer10 {
            [Hidden]
            [ScaffoldColumn(true)]
            public int NumberOfOrders {
                get { return 0; }
            }
        }

        [Test]
        public void TestDisabledWhenUntilPersistedAnnotationPickedUpOn() {
            MethodInfo actionMethod = FindMethod(typeof (Customer6), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.UntilPersisted, hiddenFacetAbstract.Value);
        }

        [Test]
        public override void TestFeatureTypes() {
            FeatureType featureTypes = facetFactory.FeatureTypes;
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.Objects));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Property));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Collections));
            Assert.IsTrue(featureTypes.HasFlag(FeatureType.Action));
            Assert.IsFalse(featureTypes.HasFlag(FeatureType.ActionParameter));
        }

        [Test]
        public void TestHiddenAnnotationPickedUpOnAction() {
            MethodInfo actionMethod = FindMethod(typeof (Customer2), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HiddenFacet);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestHiddenAnnotationPickedUpOnCollection() {
            PropertyInfo property = FindProperty(typeof (Customer1), "Orders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HiddenFacet);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestHiddenAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer), "NumberOfOrders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HiddenFacet);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestHiddenWhenAlwaysAnnotationPickedUpOn() {
            MethodInfo actionMethod = FindMethod(typeof (Customer3), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.Always, hiddenFacetAbstract.Value);
        }

        [Test]
        public void TestHiddenWhenNeverAnnotationPickedUpOn() {
            MethodInfo actionMethod = FindMethod(typeof (Customer4), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.Never, hiddenFacetAbstract.Value);
        }

        [Test]
        public void TestHiddenWhenOncePersistedAnnotationPickedUpOn() {
            MethodInfo actionMethod = FindMethod(typeof (Customer5), "SomeAction");
            facetFactory.Process(Reflector, actionMethod, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.OncePersisted, hiddenFacetAbstract.Value);
        }

        [Test]
        public void TestHiidenPriorityOverScaffoldAnnotation() {
            PropertyInfo property = FindProperty(typeof (Customer10), "NumberOfOrders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.Always, hiddenFacetAbstract.Value);
        }

        [Test]
        public void TestScaffoldAnnotationPickedUpOnCollection() {
            PropertyInfo property = FindProperty(typeof (Customer8), "Orders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HiddenFacet);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestScaffoldAnnotationPickedUpOnProperty() {
            PropertyInfo property = FindProperty(typeof (Customer7), "NumberOfOrders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            Assert.IsNotNull(facet);
            Assert.IsTrue(facet is HiddenFacet);
            AssertNoMethodsRemoved();
        }

        [Test]
        public void TestScaffoldTrueAnnotationPickedUpOn() {
            PropertyInfo property = FindProperty(typeof (Customer9), "NumberOfOrders");
            facetFactory.Process(Reflector, property, MethodRemover, Specification);
            IFacet facet = Specification.GetFacet(typeof (IHiddenFacet));
            var hiddenFacetAbstract = (HiddenFacet) facet;
            Assert.AreEqual(WhenTo.Never, hiddenFacetAbstract.Value);
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}