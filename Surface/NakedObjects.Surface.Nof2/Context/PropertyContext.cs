// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using NakedObjects.Surface;
using NakedObjects.Surface.Nof2.Wrapper;
using org.nakedobjects.@object;

namespace NakedObjects.Surface.Nof2.Context {
    public class PropertyContext : Context {
        public NakedObjectField Property { get; set; }

        public bool Mutated { get; set; }

        public override string Id {
            get { return Property.getId(); }
        }

        public override NakedObjectSpecification Specification {
            get { return Property.getSpecification(); }
        }

        public PropertyContextSurface ToPropertyContextSurface(IFrameworkFacade surface) {
            var pc = new PropertyContextSurface {
                                                    Property = new NakedObjectAssociationWrapper(Property, Target, surface),
                                                    Mutated = Mutated,
                                                };

            return ToContextSurface(pc, surface);
        }
    }
}