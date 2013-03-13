using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;

namespace UserVoice.Widgets.Services
{
    public interface IUserVoiceService : IDependency
    {
        dynamic GetSuggestionsList(string forumId = null);
        dynamic GetForumsList();
        dynamic GetClientsList();
    }
}
