﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;

namespace NakedObjects.Meta.Facet {
    [Serializable]
    public sealed class ValidateObjectFacet : FacetAbstract, IValidateObjectFacet {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ValidateObjectFacet));

        public ValidateObjectFacet(ISpecification holder, IList<NakedObjectValidationMethod> validateMethods)
            : base(Type, holder) {
            ValidateMethods = validateMethods;
        }

        public static Type Type => typeof (IValidateObjectFacet);

        private IEnumerable<NakedObjectValidationMethod> ValidateMethods { get; set; }

        #region IValidateObjectFacet Members

        public string Validate(INakedObjectAdapter nakedObjectAdapter) {
            foreach (NakedObjectValidationMethod validator in ValidateMethods) {
                var objectSpec = nakedObjectAdapter.Spec as IObjectSpec;
                Trace.Assert(objectSpec != null);

                IAssociationSpec[] matches = validator.ParameterNames.Select(name => objectSpec.Properties.SingleOrDefault(p => p.Id.ToLower() == name)).Where(s => s != null).ToArray();

                if (matches.Count() == validator.ParameterNames.Count()) {
                    INakedObjectAdapter[] parameters = matches.Select(s => s.GetNakedObject(nakedObjectAdapter)).ToArray();
                    string result = validator.Execute(nakedObjectAdapter, parameters);
                    if (result != null) return result;
                }
                else {
                    string actual = objectSpec.Properties.Select(s => s.Id).Aggregate((s, t) => s + " " + t);
                    LogNoMatch(validator, actual);
                }
            }
            return null;
        }

        public string ValidateParms(INakedObjectAdapter nakedObjectAdapter, Tuple<string, INakedObjectAdapter>[] parms) {
            foreach (NakedObjectValidationMethod validator in ValidateMethods) {
                Tuple<string, INakedObjectAdapter>[] matches = validator.ParameterNames.Select(name => parms.SingleOrDefault(p => p.Item1.ToLower() == name)).Where(p => p != null).ToArray();

                if (matches.Count() == validator.ParameterNames.Count()) {
                    INakedObjectAdapter[] parameters = matches.Select(p => p.Item2).ToArray();
                    string result = validator.Execute(nakedObjectAdapter, parameters);
                    if (result != null) return result;
                }
                else {
                    string actual = parms.Select(s => s.Item1).Aggregate((s, t) => s + " " + t);
                    LogNoMatch(validator, actual);
                }
            }
            return null;
        }

        #endregion

        private void LogNoMatch(NakedObjectValidationMethod validator, string actual) {
            string expects = validator.ParameterNames.Aggregate((s, t) => s + " " + t);
            Log.WarnFormat("No Matching parms Validator: {0} Expects {1} Actual {2} ", validator.Name, expects, actual);
        }

        #region Nested type: NakedObjectValidationMethod

        [Serializable]
        public class NakedObjectValidationMethod {
            private readonly MethodInfo method;
            [field: NonSerialized]
            private Func<object, object[], object> methodDelegate;

            public NakedObjectValidationMethod(MethodInfo method) {
                this.method = method;
                methodDelegate = DelegateUtils.CreateDelegate(method);
            }

            public string Name => method.Name;

            public string[] ParameterNames {
                get { return method.GetParameters().Select(p => p.Name.ToLower()).ToArray(); }
            }

            public string Execute(INakedObjectAdapter obj, INakedObjectAdapter[] parameters) {
                return methodDelegate(obj.GetDomainObject(), parameters.Select(no => no.GetDomainObject()).ToArray()) as string;
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context) {
                methodDelegate = DelegateUtils.CreateDelegate(method);
            }
        }

        #endregion
    }
}