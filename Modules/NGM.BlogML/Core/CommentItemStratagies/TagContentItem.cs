using System;
using System.Linq;
using BlogML.Xml;
using Orchard.ContentManagement;
using Orchard.Tags.Services;

namespace NGM.BlogML.Core.ContentTypeStrategies {
    public class TagContentItem {
        private readonly ITagService _tagService;

        public TagContentItem(ITagService tagService) {
            _tagService = tagService;
        }

        public void ImportTags(ContentItem contentItem, BlogMLBlog blogMLBlog, BlogMLPost blogMLPost) {
            var tags = blogMLPost.Categories.ToArray().Cast<BlogMLCategoryReference>()
                .Select(t => blogMLBlog.Categories.First(c => c.ID == t.Ref));

            foreach (var tag in tags) {
                if (_tagService.GetTagByName(tag.Title) == null) {
                    _tagService.CreateTag(tag.Title);
                }
            }
            _tagService.UpdateTagsForContentItem(contentItem, tags.Select(t => t.Title));
        }
    }
}