using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NGM.BlogML.Extensions;
using NGM.BlogML.Models;
using NGM.BlogML.Services;
using NGM.BlogML.ViewModels;
using Orchard;
using Orchard.Blogs.Services;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;

namespace NGM.BlogML.Controllers {
    [ValidateInput(false), Admin]
    public class AdminController : Controller {
        private readonly IImportService _importService;
        private readonly IExportService _exportService;
        private readonly IBlogService _blogService;

        public AdminController(IOrchardServices services, IImportService importService, IExportService exportService, IBlogService blogService) {
            Services = services;
            _importService = importService;
            _exportService = exportService;
            _blogService = blogService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Import() {
            if (!Services.Authorizer.Authorize(Permissions.ImportBlog, T("Cannot Import Blog")))
                return new HttpUnauthorizedResult();

            var blogs = _blogService.Get().Select(o => new KeyValuePair<int, string>(o.Id, o.Name)).ToList().AsEnumerable();
            
            return View(new ImportAdminViewModel { Model = new ImportPart { SlugPattern = @"/([^/]+)\.aspx" }, Blogs = blogs });
        }

        [HttpPost, ActionName("Import")]
        public ActionResult _Import(FormCollection formCollection) {
            if (!Services.Authorizer.Authorize(Permissions.ImportBlog, T("Cannot Import Blog")))
                return new HttpUnauthorizedResult();
            
            var viewModel = new ImportAdminViewModel();
            UpdateModel(viewModel, formCollection);

            if (ModelState.IsValid) {
                if (!string.IsNullOrWhiteSpace(viewModel.Model.URLItemPath)) {
                    if (viewModel.Model.URLItemPath.IsValidUrl())
                        _importService.Import(viewModel.Model.URLItemPath, viewModel.Model);
                    else
                        ModelState.AddModelError("File", T("Invalid Url specified").ToString());
                }
                else if (!string.IsNullOrWhiteSpace(Request.Files[0].FileName)) {
                    foreach (HttpPostedFileBase file in from string fileName in Request.Files select Request.Files[fileName]) {
                        _importService.Import(file, viewModel.Model);
                    }
                }
                else
                    ModelState.AddModelError("File", T("Select a file to upload").ToString());
            }

            viewModel.Blogs = _blogService.Get().Select(o => new KeyValuePair<int, string>(o.Id, o.Name)).ToList().AsEnumerable();
            return View(viewModel);
        }

        public ActionResult Export(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ExportBlog, T("Cannot Export Blog")))
                return new HttpUnauthorizedResult();

            _exportService.Export(id);

            return Redirect("~/Admin/Blogs");
        }
    }
}