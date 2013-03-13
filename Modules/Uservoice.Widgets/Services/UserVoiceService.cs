using System.Net;
using System.Web.Helpers;
using Orchard;
using Orchard.ContentManagement;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets.Services
{
    public class UserVoiceService : IUserVoiceService
    {
        private const string USERVOICE_API = "http://{0}.uservoice.com/api/v1/{1}.json?client={2}";

        private readonly IOrchardServices _services;
        public UserVoiceService(IOrchardServices services)
        {
            _services = services;
        }

        public dynamic GetSuggestionsList(string forumId = null)
        {
            var requestUrl = CreateRequestUrl("forums/" + forumId + "/suggestions");
            var jsonResponse = GetJsonResponse(requestUrl);
            return jsonResponse;
        }

        public dynamic GetClientsList()
        {
            var requestUrl = CreateRequestUrl("clients");
            var jsonResponse = GetJsonResponse(requestUrl);
            return jsonResponse;
        }

        public dynamic GetForumsList()
        {
            var requestUrl = CreateRequestUrl("forums");
            var jsonResponse = GetJsonResponse(requestUrl);
            return jsonResponse;

        }

        private string CreateRequestUrl(string resourcePath)
        {
            var settings = GetUserVoiceSettings();
            var requestUrl = string.Format(USERVOICE_API, settings.Account, resourcePath, settings.ApiKey);
            return requestUrl;
        }

        private SiteSettingsPart GetUserVoiceSettings()
        {
            var settings = _services.WorkContext.CurrentSite.As<SiteSettingsPart>();
            return settings;
        }

        private dynamic GetJsonResponse(string apiUrl)
        {
            WebClient client = new WebClient();
            return Json.Decode(client.DownloadString(apiUrl));
        }
    }
}