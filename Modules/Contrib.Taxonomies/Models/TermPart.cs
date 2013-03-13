using System;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;
using Orchard.ContentManagement.Aspects;

namespace Contrib.Taxonomies.Models {
    public class TermPart : ContentPart<TermPartRecord>, IComparable<TermPart> {
        public string Name {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<RoutePart>().Slug; }
            set { this.As<RoutePart>().Slug = value; }
        }

        public IContent Container {
            get { return this.As<ICommonPart>().Container; }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int TaxonomyId {
            get { return Record.TaxonomyId; }
            set { Record.TaxonomyId = value; }
        }

        public string Path {
            get { return Record.Path; }
            set { Record.Path = value; }
        }

        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }

        public bool Selectable {
            get { return Record.Selectable; }
            set { Record.Selectable = value; }
        }

        public int Weight {
            get { return Record.Weight; }
            set { Record.Weight = value; }
        }

        public string FullPath { get { return String.Concat(Path, "/", Id); } }

        #region IComparable<TermPart> Members

        public int CompareTo(TermPart other) {
            return other.Path == this.Path ? Weight.CompareTo(other.Weight) : FullPath.CompareTo(other.FullPath);
        }

        #endregion
    }
}