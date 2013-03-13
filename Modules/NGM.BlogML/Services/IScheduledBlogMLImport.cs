using System.Collections.Generic;
using BlogML.Xml;
using NGM.BlogML.Models;
using Orchard.Blogs.Models;
using Orchard.Events;

namespace NGM.BlogML.Services {
    public interface IScheduledBlogMLImport : IEventHandler {
        void ImportBatch(IEnumerable<BlogMLPost> batch, ImportPart importPart, BlogMLBlog blogMLBlog, int? parentBlogId);
    }
}
