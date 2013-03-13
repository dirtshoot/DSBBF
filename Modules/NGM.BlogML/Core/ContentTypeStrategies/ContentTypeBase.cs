using System;
using Orchard;

namespace NGM.BlogML.Core.ContentTypeStrategies {
    public abstract class ContentTypeBase {
        public ContentTypeBase(IOrchardServices orchardServices) {
            Services = orchardServices;
        }

        public IOrchardServices Services { get; private set; }

        public virtual KnownContentType ContentType {
            get { throw new NotSupportedException(); }
        }
    }
}