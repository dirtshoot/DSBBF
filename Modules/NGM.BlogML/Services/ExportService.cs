using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BlogML;
using JetBrains.Annotations;
using NGM.BlogML.Core.Persistance;
using NGM.BlogML.Models;
using Orchard;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Tags.Services;
using Orchard.UI.Notify;

namespace NGM.BlogML.Services {
    public class ExportService : BlogMLWriterBase, IExportService {

        private readonly ICommentService _commentService;
        private readonly ITagService _tagService;
        private readonly IStorageProvider _storageProvider;

        private const string _blogContentType = "Blog";
        private const string _blogPostContentType = "BlogPost";

        private int _blogId;
        private string _blogFileName;

        public ExportService(IOrchardServices services,
            ICommentService commentService, 
            ITagService tagService, 
            IStorageProvider storageProvider) {
            Services = services;
            _commentService = commentService;
            _tagService = tagService;
            _storageProvider = storageProvider;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public void Export(int id) {
            _blogId = id;

            var stream = new MemoryStream();
            var writer = new XmlTextWriter(stream, new UTF8Encoding(false));
            try {
                Write(writer);

                var filePersistance = new FilePersistance();

                var fileNameAndPath = CreateFileNameAndPath();

                filePersistance.UploadMediaFile(fileNameAndPath, stream.ToArray());

                Services.Notifier.Information(T(string.Format("Blog has been exported to {0}", fileNameAndPath)));
            } catch {
                Services.Notifier.Information(T("An error has occurred exporting blog"));
            }
        }

        [NotNull]
        public string CreateFileNameAndPath() {
            var settingsBlogML = Services.WorkContext.CurrentSite.As<BlogMLSettingsPart>().Record;
            var pathCombined = _storageProvider.GetPublicUrl(settingsBlogML.ExportDirectory).Replace("\\", "/");
            return Path.Combine(pathCombined, _blogFileName).Replace("\\", "/");
        }

        protected override void InternalWriteBlog() {
            var blogContentItem = Services.ContentManager.Get(_blogId);

            WriteStartBlog(blogContentItem.As<RoutePart>().Title,
                           ContentTypes.Text,
                           blogContentItem.As<RoutePart>().Title,
                           ContentTypes.Text,
                           blogContentItem.As<RoutePart>().Slug,
                           blogContentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault());

            // Do all authors
            WriteAuthors(blogContentItem);

            //TODO : What are extended properties?

            // All Categories
            WriteCategories(blogContentItem);

            WritePosts(blogContentItem);

            WriteEndElement();
            Writer.Flush();

            _blogFileName =
                string.Format("{0}_{1:yyyy-MM-dd_hh-mm-ss-tt}.xml",
                blogContentItem.As<RoutePart>().Slug,
                DateTime.UtcNow);
        }

        private void WriteAuthors(IContent contentItem) {
            WriteStartAuthors();
            
            // TODO clean Up
            var listUniqueOwnerIds = new List<int>();

            WriteAuthor(contentItem.As<CommonPart>().Owner.Id.ToString(CultureInfo.InvariantCulture),
                        contentItem.As<CommonPart>().Owner.UserName,
                        contentItem.As<CommonPart>().Owner.Email,
                        contentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                        contentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                        true);

            listUniqueOwnerIds.Add(contentItem.As<CommonPart>().Owner.Id);

            var query = Services.ContentManager.Query(VersionOptions.Published, _blogPostContentType)
                .Join<CommonPartRecord>()
                .Where(cr => cr.Container == contentItem.ContentItem.Record)
                .OrderByDescending(cr => cr.CreatedUtc);

            var blogPostContentItems = query.List();

            foreach (var blogPostContentItem in blogPostContentItems) {
                if (listUniqueOwnerIds.Contains(blogPostContentItem.As<CommonPart>().Owner.Id))
                    break;

                WriteAuthor(blogPostContentItem.As<CommonPart>().Owner.Id.ToString(CultureInfo.InvariantCulture),
                            blogPostContentItem.As<CommonPart>().Owner.UserName,
                            blogPostContentItem.As<CommonPart>().Owner.Email,
                            blogPostContentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                            blogPostContentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                            true);

                listUniqueOwnerIds.Add(blogPostContentItem.As<CommonPart>().Owner.Id);
            }

            WriteEndElement();
        }

        private void WriteCategories(IContent contentItem) {
            WriteStartCategories();

            foreach (var tag in _tagService.GetTags()) {
                var taggedContentItem = _tagService.GetTaggedContentItems(tag.Id).Where(t => t.ContentItem.Id == contentItem.ContentItem.Id);

                if (taggedContentItem != null)
                    WriteCategory(tag.Id.ToString(), 
                        tag.TagName,
                        contentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                        contentItem.As<CommonPart>().ModifiedUtc.GetValueOrDefault(),
                        contentItem.IsPublished(),
                        string.Empty,
                        string.Empty);
            }

            WriteEndElement();
        }

        private void WritePosts(IContent contentItem) {
            WriteStartPosts();

            var query = Services.ContentManager.Query(VersionOptions.Published, _blogPostContentType)
                .Join<CommonPartRecord>()
                .Where(cr => cr.Container == contentItem.ContentItem.Record)
                .OrderByDescending(cr => cr.CreatedUtc);

            var blogPostContentItems = query.List();

            foreach (var blogPostContentItem in blogPostContentItems) {
                // I dont think we have a Excerpt???
                WriteStartPost(
                    blogPostContentItem.Id.ToString(CultureInfo.InvariantCulture),
                    blogPostContentItem.As<RoutePart>().Title,
                    blogPostContentItem.As<CommonPart>().CreatedUtc.GetValueOrDefault(),
                    blogPostContentItem.As<CommonPart>().ModifiedUtc.GetValueOrDefault(),
                    blogPostContentItem.VersionRecord.Published,
                    blogPostContentItem.As<BodyPart>().Text,
                    blogPostContentItem.As<RoutePart>().Slug, // TODO : FULL URI!
                    0,
                    BlogPostTypes.Normal, // No other types can be done yet...
                    blogPostContentItem.As<RoutePart>().Title);

                WritePostCategories(blogPostContentItem);
                WritePostComments(blogPostContentItem);
                // NO Trackbacks
                // NO Attachments
                WritePostAuthors(blogPostContentItem);

                WriteEndElement();
            }

            WriteEndElement();
        }

        private void WritePostCategories(IContent contentItem) {
            var tagIdentifiers = new List<int>();

            foreach (var tag in _tagService.GetTags()) {
                var taggedContentItem = _tagService.GetTaggedContentItems(tag.Id).Where(t => t.ContentItem.Id == contentItem.ContentItem.Id);

                if (taggedContentItem != null)
                    tagIdentifiers.Add(tag.Id);
            }

            if (tagIdentifiers.Count == 1) {
                return;
            }

            WriteStartCategories();
            foreach (var tagId in tagIdentifiers) {
                WriteCategoryReference(tagId.ToString(CultureInfo.InvariantCulture));
            }
            WriteEndElement();
        }

        private void WritePostComments(IContent contentItem) {
            var comments = _commentService.GetCommentsForCommentedContent(contentItem.ContentItem.Id);

            if (comments.Count() == 0)
                return;
            
            WriteStartComments();
            foreach (var comment in comments.List()) {
                WriteComment(comment.Record.Id.ToString(CultureInfo.InvariantCulture),
                             string.Empty,
                             comment.Record.CommentDateUtc.GetValueOrDefault(),
                             comment.Record.CommentDateUtc.GetValueOrDefault(),
                             comment.Record.Status == CommentStatus.Approved ? true : false,
                             comment.Record.Author,
                             comment.Record.Email,
                             comment.Record.SiteName,
                             comment.Record.CommentText);
            }
            WriteEndElement();
        }

        private void WritePostAuthors(IContent contentItem) {
            WriteStartAuthors();
            WriteAuthorReference(contentItem.As<CommonPart>().Owner.Id.ToString(CultureInfo.InvariantCulture));
            WriteEndElement();
        }
    }
}