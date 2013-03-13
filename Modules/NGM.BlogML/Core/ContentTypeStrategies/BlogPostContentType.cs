using System.IO;
using System.Web;
using BlogML;
using BlogML.Xml;
using JetBrains.Annotations;
using NGM.BlogML.Core.CommentItemStratagies;
using NGM.BlogML.Core.Persistance;
using NGM.BlogML.Extensions;
using NGM.BlogML.Models;
using Orchard;
using Orchard.Blogs.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Reports.Services;
using Orchard.Tags.Services;

namespace NGM.BlogML.Core.ContentTypeStrategies {
    public class BlogPostContentType : ContentTypeBase {
        private readonly IRoutableService _routableService;
        private readonly IReportsCoordinator _reportsCoordinator;
        private readonly IStorageProvider _storageProvider;
        private readonly ICommentService _commentService;
        private readonly ITagService _tagService;

        private BlogPart _partentBlogPart;

        public BlogPostContentType(IRoutableService routableService, 
            IReportsCoordinator reportsCoordinator, 
            IStorageProvider storageProvider, 
            ICommentService commentService,
            ITagService tagService,
            IOrchardServices orchardServices) : base(orchardServices) {
            _routableService = routableService;
            _reportsCoordinator = reportsCoordinator;
            _storageProvider = storageProvider;
            _commentService = commentService;
            _tagService = tagService;
        }

        public override KnownContentType ContentType {
            get {
                return KnownContentType.BlogPost;
            }
        }
        
        public Localizer T { get; set; }

        public BlogPostPart Import(ImportPart importPart, BlogPart parentBlogPart, BlogMLBlog blogMlBlog, BlogMLPost blogMLPost) {
            _partentBlogPart = parentBlogPart;

            var blogPost = Services.ContentManager.New<BlogPostPart>(ContentType.ToString());
            MapValuesToCommonPart(blogPost.As<CommonPart>(), blogMLPost);
            MapValuesToRoutePart(blogPost.As<RoutePart>(), importPart, blogMLPost);
            var postContent = blogMLPost.Content.Text;
            if (blogMLPost.Attachments.Count > 0) {
                // Updates the post content with new attachment urls.
                postContent = ImportAndBuildAttachmentContent(blogMLPost, postContent);
            }
            
            MapContentToBodyPart(blogPost.As<BodyPart>(), postContent);
            Services.ContentManager.Create(blogPost, VersionOptions.Draft);

            if (blogMLPost.Approved) {
                Services.ContentManager.Publish(blogPost.ContentItem);
                MapValuesToCommonPart(blogPost.As<CommonPart>(), blogMLPost);
            }
            _reportsCoordinator.Information("Blog Migration", string.Format("Blogpost '{0}' has been imported with the slug of '{1}'", blogMLPost.Title, blogPost.As<RoutePart>().Slug));

            ImportRelatedContentItems(importPart, blogMlBlog, blogMLPost, blogPost);

            return blogPost;
        }

        private void MapValuesToRoutePart(RoutePart routePart, ImportPart import, BlogMLPost blogMLPost) {
            routePart.Title = blogMLPost.Title;

            routePart.Slugify(_routableService, import.SlugPattern, blogMLPost.PostUrl);
        }

        private void MapValuesToCommonPart(CommonPart commonPart, BlogMLPost blogMLPost) {
            // Hopefully override the publish datetime
            commonPart.Container = _partentBlogPart;

            commonPart.CreatedUtc = blogMLPost.DateCreated;
            commonPart.VersionCreatedUtc = blogMLPost.DateCreated;
            if (blogMLPost.DateModified == (new System.DateTime())) {
                commonPart.ModifiedUtc = blogMLPost.DateCreated;
                commonPart.VersionModifiedUtc = blogMLPost.DateCreated;
            }
            else {
                commonPart.ModifiedUtc = blogMLPost.DateModified;
                commonPart.PublishedUtc = blogMLPost.DateModified;
                commonPart.VersionModifiedUtc = blogMLPost.DateModified;
                commonPart.VersionPublishedUtc = blogMLPost.DateModified;
            }
        }

        private void MapContentToBodyPart(BodyPart bodyPart, string content) {
            bodyPart.Text = content;
        }

        [NotNull]
        private string ImportAndBuildAttachmentContent(BlogMLPost blogMLPost, string postContent) {
            var filePersistance = new FilePersistance();

            foreach (BlogMLAttachment blogMLAttachment in blogMLPost.Attachments) {
                if (!blogMLAttachment.Embedded)
                    continue;

                var attachmentUrl = GetFileAttachmentUrl(Path.GetFileName(blogMLAttachment.Url));

                postContent = BlogMLWriterBase.SgmlUtil.CleanAttachmentUrls(
                    postContent,
                    blogMLAttachment.Url,
                    attachmentUrl);

                filePersistance.UploadMediaFile(
                    attachmentUrl,
                    blogMLAttachment.Data);
            }
            return postContent;
        }

        [CanBeNull]
        public string GetFileAttachmentUrl(string fileUrl) {
            return Path.Combine(GetAttachmentUrl(), fileUrl).Replace("\\", "/");
        }

        [NotNull]
        public string GetAttachmentUrl() {
            var settingsBlogML = Services.WorkContext.CurrentSite.As<BlogMLSettingsPart>().Record;

            return Path.Combine(HttpContext.Current.Request.ApplicationPath,
                _storageProvider.GetPublicUrl(settingsBlogML.AttachmentDirectory).Replace("\\", "/")).Replace("\\", "/");
        }

        protected void ImportRelatedContentItems(ImportPart importPart, BlogMLBlog blogMLBlog, BlogMLPost blogMLPost, BlogPostPart blogPostPart) {
            CommentContentItem commentContentItem = new CommentContentItem(_commentService);
            commentContentItem.ImportComments(blogPostPart.ContentItem, blogMLPost);

            TagContentItem tagContentItem = new TagContentItem(_tagService);
            tagContentItem.ImportTags(blogPostPart.ContentItem, blogMLBlog, blogMLPost);
        }
    }
}