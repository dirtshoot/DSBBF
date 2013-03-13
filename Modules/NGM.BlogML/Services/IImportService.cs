using System.Web;
using NGM.BlogML.Models;
using Orchard;

namespace NGM.BlogML.Services {
    public interface IImportService : IDependency {
        void Import(HttpPostedFileBase httpPostedFileBase, ImportPart importPart);

        void Import(string urlItemPath, ImportPart model);
    }
}
