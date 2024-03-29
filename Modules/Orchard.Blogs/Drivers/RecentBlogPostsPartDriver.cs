﻿using System;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Drivers {
    public class RecentBlogPostsPartDriver : ContentPartDriver<RecentBlogPostsPart> {
        private readonly IBlogService _blogService;
        private readonly IContentManager _contentManager;
        private readonly IBlogPathConstraint _blogPathConstraint;

        public RecentBlogPostsPartDriver(
            IBlogService blogService, 
            IContentManager contentManager,
            IBlogPathConstraint blogPathConstraint) {
            _blogService = blogService;
            _contentManager = contentManager;
            _blogPathConstraint = blogPathConstraint;
        }

        protected override DriverResult Display(RecentBlogPostsPart part, string displayType, dynamic shapeHelper) {
            var path = _blogPathConstraint.FindPath(part.ForBlog);
            BlogPart blog = _blogService.Get(path);

            if (blog == null) {
                return null;
            }

            var blogPosts =_contentManager.Query(VersionOptions.Published, "BlogPost")
                .Join<CommonPartRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord)
                .OrderByDescending(cr => cr.CreatedUtc)
                .Slice(0, part.Count)
                .Select(ci => ci.As<BlogPostPart>());

            var list = shapeHelper.List();
            list.AddRange(blogPosts.Select(bp => _contentManager.BuildDisplay(bp, "Summary")));

            var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, ContentItems: list);

            return ContentShape(shapeHelper.Parts_Blogs_RecentBlogPosts(ContentItem: part.ContentItem, ContentItems: blogPostList));
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, dynamic shapeHelper) {
            var viewModel = new RecentBlogPostsViewModel {
                Count = part.Count,
                Path = part.ForBlog,
                Blogs = _blogService.Get().ToList().OrderBy(b => b.Name)
            };

            return ContentShape("Parts_Blogs_RecentBlogPosts_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.RecentBlogPosts", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new RecentBlogPostsViewModel();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.ForBlog = viewModel.Path;
                part.Count = viewModel.Count;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(RecentBlogPostsPart part, ImportContentContext context) {
            var blogSlug = context.Attribute(part.PartDefinition.Name, "BlogSlug");
            if (blogSlug != null) {
                part.ForBlog = blogSlug;
            }

            var count = context.Attribute(part.PartDefinition.Name, "Count");
            if (count != null) {
                part.Count = Convert.ToInt32(count);
            }
        }

        protected override void Exporting(RecentBlogPostsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("BlogSlug", part.ForBlog);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
        }
    }
}