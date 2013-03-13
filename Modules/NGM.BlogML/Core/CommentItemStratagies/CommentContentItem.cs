using BlogML.Xml;
using NGM.BlogML.Extensions;
using Orchard.Comments.Services;
using Orchard.ContentManagement;

namespace NGM.BlogML.Core.CommentItemStratagies {
    public class CommentContentItem {
        private readonly ICommentService _commentService;

        public CommentContentItem(ICommentService commentService) {
            _commentService = commentService;
        }

        public void ImportComments(ContentItem contentItem, BlogMLPost blogMLPost) {
            foreach (BlogMLComment blogMLComment in blogMLPost.Comments) {
                var context = new CreateCommentContext {
                    Author = (blogMLComment.UserName ?? "").Truncate(255),
                    CommentText = (blogMLComment.Content.Text ?? "").Truncate(10000),
                    Email = (blogMLComment.UserEMail ?? "").Truncate(255),
                    SiteName = (blogMLComment.UserUrl ?? "").Truncate(255),
                    CommentedOn = contentItem.Id,
                };

                var comment = _commentService.CreateComment(context, true);

                comment.Record.CommentDateUtc = blogMLComment.DateCreated;
                comment.Record.UserName = blogMLComment.UserName.Truncate(255);

                if (blogMLComment.Approved)
                    _commentService.ApproveComment(comment.ContentItem.Id);
            }
        }
    }
}