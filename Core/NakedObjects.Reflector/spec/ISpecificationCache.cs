// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using NakedObjects.Architecture.Reflect;
using NakedObjects.Architecture.SpecImmutable;

namespace NakedObjects.Reflector.Spec {
    public interface ISpecificationCache {
        IObjectSpecImmutable GetSpecification(string key);

        void Clear();

        IObjectSpecImmutable[] AllSpecifications();

        void Cache(string key, IObjectSpecImmutable spec);
    }

    // Copyright (c) Naked Objects Group Ltd.
}