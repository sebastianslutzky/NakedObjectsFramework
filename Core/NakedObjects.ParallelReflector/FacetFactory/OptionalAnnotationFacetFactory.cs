// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Immutable;
using System.Reflection;
using Common.Logging;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.FacetFactory;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Meta.Facet;
using NakedObjects.Meta.Utils;
using NakedObjects.Util;

namespace NakedObjects.ParallelReflect.FacetFactory {
    public sealed class OptionalAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OptionalAnnotationFacetFactory));

        public OptionalAnnotationFacetFactory(int numericOrder)
            : base(numericOrder, FeatureType.PropertiesAndActionParameters) { }

        private static void Process(MemberInfo member, ISpecification holder) {
            var attribute = member.GetCustomAttribute<OptionallyAttribute>();
            FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override IImmutableDictionary<string, ITypeSpecBuilder> Process(IReflector reflector, MethodInfo method, IMethodRemover methodRemover, ISpecificationBuilder specification, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            if ((method.ReturnType.IsPrimitive || TypeUtils.IsEnum(method.ReturnType)) && method.GetCustomAttribute<OptionallyAttribute>() != null) {
                Log.Warn("Ignoring Optionally annotation on primitive parameter on " + method.ReflectedType + "." + method.Name);
                return metamodel;
            }

            Process(method, specification);
            return metamodel;
        }

        public override IImmutableDictionary<string, ITypeSpecBuilder> Process(IReflector reflector, PropertyInfo property, IMethodRemover methodRemover, ISpecificationBuilder specification, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            if ((property.PropertyType.IsPrimitive || TypeUtils.IsEnum(property.PropertyType)) && property.GetCustomAttribute<OptionallyAttribute>() != null) {
                Log.Warn("Ignoring Optionally annotation on primitive or un-readable parameter on " + property.ReflectedType + "." + property.Name);
                return metamodel;
            }

            if (property.GetGetMethod() != null && !property.PropertyType.IsPrimitive) {
                Process(property, specification);
            }

            return metamodel;
        }

        public override IImmutableDictionary<string, ITypeSpecBuilder> ProcessParams(IReflector reflector, MethodInfo method, int paramNum, ISpecificationBuilder holder, IImmutableDictionary<string, ITypeSpecBuilder> metamodel) {
            ParameterInfo parameter = method.GetParameters()[paramNum];
            if (parameter.ParameterType.IsPrimitive || TypeUtils.IsEnum(parameter.ParameterType)) {
                if (method.GetCustomAttribute<OptionallyAttribute>() != null) {
                    Log.Warn("Ignoring Optionally annotation on primitive parameter " + paramNum + " on " + method.ReflectedType + "." + method.Name);
                }

                return metamodel;
            }

            var attribute = parameter.GetCustomAttribute<OptionallyAttribute>();
            FacetUtils.AddFacet(Create(attribute, holder));
            return metamodel;
        }

        private static IMandatoryFacet Create(OptionallyAttribute attribute, ISpecification holder) {
            return attribute != null ? new OptionalFacet(holder) : null;
        }
    }
}