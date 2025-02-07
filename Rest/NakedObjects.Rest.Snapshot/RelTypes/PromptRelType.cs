// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Net.Http.Headers;
using NakedObjects.Rest.Snapshot.Constants;

namespace NakedObjects.Rest.Snapshot.Utility {
    public class PromptRelType : RelType {
        public PromptRelType(UriMtHelper helper) : base(RelValues.Prompt, helper) {}
        public PromptRelType(string name, UriMtHelper helper) : base(name, helper) {}

        public override string Name => base.Name + (HasRelParameter ? Helper.GetRelParameters() : "");

        public override Uri GetUri() {
            return Helper.GetPromptUri();
        }

        public override MediaTypeHeaderValue GetMediaType(RestControlFlags flags) {
            MediaTypeHeaderValue mediaType = UriMtHelper.GetJsonMediaType(RepresentationTypes.Prompt);
            return mediaType;
        }
    }
}