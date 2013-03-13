using System.Web.Mvc;
using Orchard.ContentManagement;

namespace NGM.BlogML.Extensions {
    public static class UrlHelperExtensions {
        public static string BlogExport(this UrlHelper urlHelper, ContentItem contentItem) {
            return urlHelper.Action("Export", "Admin", new { id = contentItem.Id, area = "NGM.BlogML" });
        }
    }
}