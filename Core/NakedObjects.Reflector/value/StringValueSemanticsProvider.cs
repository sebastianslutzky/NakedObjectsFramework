// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Globalization;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.Spec;
using NakedObjects.Architecture.SpecImmutable;
using NakedObjects.Capabilities;

namespace NakedObjects.Reflector.DotNet.Value {
    public class StringValueSemanticsProvider : ValueSemanticsProviderAbstract<string>, IStringValueFacet {
        private const string defaultValue = null;
        private const bool equalByContent = true;
        private const bool immutable = true;
        private const int typicalLength = 25;

        /// <summary>
        ///     Required because implementation of <see cref="IParser{T}" /> and <see cref="IEncoderDecoder{T}" />.
        /// </summary>
        public StringValueSemanticsProvider(IObjectSpecImmutable spec)
            : this(spec, null) {}

        public StringValueSemanticsProvider(IObjectSpecImmutable spec, ISpecification holder)
            : base(Type, holder, AdaptedType, typicalLength, immutable, equalByContent, defaultValue, spec) {}

        public static Type Type {
            get { return typeof (IStringValueFacet); }
        }

        public static Type AdaptedType {
            get { return typeof (string); }
        }

        public string StringValue(INakedObject nakedObject) {
            return nakedObject.GetDomainObject<string>();
        }

        public static bool IsAdaptedType(Type type) {
            return type == typeof (string);
        }


        protected override string DoParse(string entry) {
            if (entry.Trim().Equals("")) {
                return null;
            }
            return entry;
        }

        protected override string DoParseInvariant(string entry) {
            return entry;
        }

        protected override string GetInvariantString(string obj) {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        protected override string DoEncode(string obj) {
            string text = obj;
            if (text.Equals("NULL") || IsEscaped(text)) {
                return EscapeText(text);
            }
            return text;
        }

        protected override string DoRestore(string data) {
            if (IsEscaped(data)) {
                return data.Substring(1);
            }
            return data;
        }

        private static bool IsEscaped(string text) {
            return text.StartsWith("/");
        }

        private static string EscapeText(string text) {
            return "/" + text;
        }
    }
}