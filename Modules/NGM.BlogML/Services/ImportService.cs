using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using BlogML.Xml;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using NGM.BlogML.Core.ContentTypeStrategies;
using NGM.BlogML.Models;
using Orchard;
using Orchard.Core.Routable.Services;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Reports.Services;
using Orchard.Settings;
using Orchard.Tasks;
using Orchard.UI.Notify;

namespace NGM.BlogML.Services {
    public class ImportService : IImportService {

        private readonly IRoutableService _routableService;
        private readonly IBackgroundTask _backgroundTask;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IOrchardServices _orchardServices;

        public ImportService(IOrchardServices services,
            IRoutableService routableService, 
            IBackgroundTask backgroundTask,
            IReportsCoordinator reportsCoordinator,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IOrchardServices orchardServices) {
            Services = services;
            _routableService = routableService;
            _backgroundTask = backgroundTask;
            _reportsCoordinator = reportsCoordinator;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        #region IImportService Members

        public void Import(HttpPostedFileBase httpPostedFileBase, ImportPart importPart) {
            if (importPart == null)
                throw new ArgumentNullException("importPart");

            var blogsToImport = BuildBlogMLArray(httpPostedFileBase);
            ImportBlogs(blogsToImport, importPart);
        }

        public void Import(string urlItemPath, ImportPart importPart) {
            var client = new WebClient();
            Stream stream = null;
            try {
                stream = client.OpenRead(urlItemPath);

                try {
                    var blogToImport = DeserializeBlogMlByStream(stream);
                    if (blogToImport == null)
                        Services.Notifier.Information(T("No blog to import"));
                    else
                        ImportBlogs(new[] { blogToImport }, importPart);
                } catch {
                    Services.Notifier.Error(T("An error occured importing your blog"));
                }
            } catch (Exception ex) {
                Services.Notifier.Error(T("An error occured loading your blog, please check permissions and file is accessible, error: {0}", ex.Message));
            } finally {
                if (stream != null)
                    stream.Close();
                client.Dispose();
            }
        }

        private void ImportBlogs(IEnumerable<BlogMLBlog> blogsToImport, ImportPart importPart) {
            _reportsCoordinator.Register("Blog Migration", "Import Blog" + DateTime.Now, "BlogML Import");

            foreach (var blogMLBlog in blogsToImport) {
                var blogContentType = new BlogContentType(
                    _routableService,
                    _reportsCoordinator,
                    _processingEngine,
                    _shellSettings,
                    _shellDescriptorManager,
                    _orchardServices);
                blogContentType.Import(importPart, blogMLBlog);
            }

            RebuildOrchardIndexes();

            Services.Notifier.Information(T("Blog Import has completed successfully. All errors will be listed below, and you may review report generated via the reports link in the menu at anytime."));
        }

        #endregion

        private void RebuildOrchardIndexes() {
            _backgroundTask.Sweep();
        }

        private BlogMLBlog[] BuildBlogMLArray(HttpPostedFileBase httpPostedFileBase) {
            var blogMLBlogs = new BlogMLBlog[0];

            if (httpPostedFileBase.FileName.EndsWith(".zip")) {
                blogMLBlogs = UnzipMediaFileArchiveToBlogMLBlog(httpPostedFileBase);
            } else if ((httpPostedFileBase.FileName.EndsWith(".xml"))) {
                blogMLBlogs = new[] { DeserializeBlogMlByStream(httpPostedFileBase.InputStream) };
            }

            return blogMLBlogs;
        }

        private BlogMLBlog[] UnzipMediaFileArchiveToBlogMLBlog(HttpPostedFileBase postedFile) {
            var postedFileLength = postedFile.ContentLength;
            var postedFileStream = postedFile.InputStream;
            var postedFileData = new byte[postedFileLength];
            postedFileStream.Read(postedFileData, 0, postedFileLength);

            var blogMlBlogs = new List<BlogMLBlog>();

            using (var memoryStream = new MemoryStream(postedFileData)) {
                var fileInflater = new ZipInputStream(memoryStream);
                ZipEntry entry;
                while ((entry = fileInflater.GetNextEntry()) != null) {
                    if (entry.IsDirectory || entry.Name.Length <= 0)
                        continue;
                    
                    if (entry.IsFile && entry.Name.EndsWith(".xml"))
                        blogMlBlogs.Add(DeserializeBlogMlByStream(fileInflater));
                }
            }

            return blogMlBlogs.ToArray();
        }

        [CanBeNull]
        private BlogMLBlog DeserializeBlogMlByStream(Stream stream) {
            try {
                return BlogMLSerializer.Deserialize(stream);
            }
            catch (Exception ex) {
                Services.Notifier.Error(T("Error deserializing your blog Error:{0} - Please verify that this is an XML file stream", ex.Message));
                throw;
            }
        }
    }
}