using System.Collections.Generic;
using System.Linq;
using BlogML.Xml;
using NGM.BlogML.Extensions;
using NGM.BlogML.Models;
using Orchard;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Reports.Services;

namespace NGM.BlogML.Core.ContentTypeStrategies {
    public class BlogContentType : ContentTypeBase, IDependency {
        private readonly IRoutableService _routableService;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        private const int BatchSize = 100;

        public BlogContentType(IRoutableService routableService, 
            IReportsCoordinator reportsCoordinator, 
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IOrchardServices orchardServices) : base(orchardServices) {
            _routableService = routableService;
            _reportsCoordinator = reportsCoordinator;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public override KnownContentType ContentType {
            get {
                return KnownContentType.Blog;
            }
        }
        
        public Localizer T { get; set; }

        public BlogPart Import(ImportPart importPart, BlogMLBlog blogMLBlog) {
                BlogPart blog;
                if (importPart.BlogIdToImportInto.HasValue && importPart.BlogIdToImportInto.Value >= 1) {
                    blog = Services.ContentManager.Get<BlogPart>(importPart.BlogIdToImportInto.Value);
                }
                else {
                    blog = Services.ContentManager.New<BlogPart>(ContentType.ToString());

                    MapValuesToRoutePart(blog.As<RoutePart>(), importPart, blogMLBlog);

                    Services.ContentManager.Create(blog);

                    MapValuesToCommonPart(blog.As<CommonPart>(), blogMLBlog);

                    Services.ContentManager.Publish(blog.ContentItem);

                    _reportsCoordinator.Information("Blog Migration", string.Format("Blog '{0}' has been imported to the slug of '{1}'", blogMLBlog.Title, blog.As<RoutePart>().Slug));
                }

                ImportChildContentItems(importPart, blogMLBlog, blog);
                return blog;
        }

        private void MapValuesToRoutePart(RoutePart routePart, ImportPart import, BlogMLBlog blogMLBlog) {
            routePart.Title = blogMLBlog.Title;

            if (!string.IsNullOrEmpty(import.DefaultBlogSlug))
                routePart.Slug = import.DefaultBlogSlug;

            routePart.Slugify(_routableService, import.SlugPattern, blogMLBlog.RootUrl);
        }

        private static void MapValuesToCommonPart(CommonPart commonPart, BlogMLBlog blogMLBlog) {
            commonPart.CreatedUtc = blogMLBlog.DateCreated;
            commonPart.VersionCreatedUtc = blogMLBlog.DateCreated;
        }

        protected void ImportChildContentItems(ImportPart importPart, BlogMLBlog blogMLBlog, BlogPart parentBlogPart) {
            var posts = blogMLBlog.Posts.Where(IsValidBlogPostImport).ToList();
            for (var i = 0; i < posts.Count; i+= BatchSize) {
                var postBatch = posts.Skip(i).Take(BatchSize).ToList();
                _processingEngine.AddTask(
                    _shellSettings,
                    _shellDescriptorManager.GetShellDescriptor(),
                    "IScheduledBlogMLImport.ImportBatch",
                    new Dictionary<string, object> {
                        {"batch", postBatch},
                        {"importPart", importPart},
                        {"blogMLBlog", blogMLBlog},
                        {"parentBlogId", parentBlogPart.Id}
                    });
            }
        }

        // TODO : Extension Method?
        private static bool IsValidBlogPostImport(BlogMLPost post) {
            if (string.IsNullOrEmpty(post.Title))
                return false;

            if (string.IsNullOrEmpty(post.Content.Text))
                return false;

            return true;
        }
    }
}