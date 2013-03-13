using JetBrains.Annotations;
using NGM.BlogML.Models;
using NGM.BlogML.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.BlogML.Drivers {
    [UsedImplicitly]
    public class ExportContainerPartDriver : ContentPartDriver<ExportContainerPart> {
        protected override string Prefix {
            get { return "BlogML"; }
        }

        protected override DriverResult Display(ExportContainerPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "DetailAdmin") {
                return ContentShape("Parts_BlogML_Export",
                                    () => shapeHelper.Parts_BlogML_Export(ContentPart: part));
            }
            return null;
        }
    }
}