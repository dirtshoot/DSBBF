using System.Collections.Generic;
using BlogML.Xml;
using NGM.BlogML.Core.ContentTypeStrategies;
using NGM.BlogML.Models;
using Orchard;
using Orchard.Blogs.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Services;
using Orchard.FileSystems.Media;
using Orchard.Reports.Services;
using Orchard.Tags.Services;

namespace NGM.BlogML.Services {
    public class ScheduledBlogMLImport : IScheduledBlogMLImport {
        private readonly IRoutableService _routableService;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IStorageProvider _storageProvider;
        private readonly ICommentService _commentService;
        private readonly ITagService _tagService;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;

        public ScheduledBlogMLImport(
            IRoutableService routableService,
            IReportsCoordinator reportsCoordinator,
            IStorageProvider storageProvider,
            ICommentService commentService,
            ITagService tagService,
            IContentManager contentManager,
            IOrchardServices orchardServices) {

            _routableService = routableService;
            _reportsCoordinator = reportsCoordinator;
            _storageProvider = storageProvider;
            _commentService = commentService;
            _tagService = tagService;
            _contentManager = contentManager;
            _services = orchardServices;
        }

        public void ImportBatch(IEnumerable<BlogMLPost> batch, ImportPart importPart, BlogMLBlog blogMLBlog, int? parentBlogId) {
            var blogPostContentType = new BlogPostContentType(_routableService, _reportsCoordinator, _storageProvider, _commentService, _tagService, _services);
            var parentBlogPart = parentBlogId == null ? null : _contentManager.Get<BlogPart>((int)parentBlogId);

            foreach (var blogMlPost in batch) {
                blogPostContentType.Import(importPart, parentBlogPart, blogMLBlog, blogMlPost);
            }
        }
    }
}
