using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using UserVoice.Widgets.Models;
using UserVoice.Widgets.Services;
using UserVoice.Widgets.ViewModels;

namespace UserVoice.Widgets.Drivers
{
    public class SuggestionsPartDriver : ContentPartDriver<SuggestionsPart>
    {
        private readonly IUserVoiceService _userVoiceService;
        private readonly IOrchardServices _orchardServices;
        public SuggestionsPartDriver(IOrchardServices orchardServices,
                                     IUserVoiceService userVoiceService)
        {
            _userVoiceService = userVoiceService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override DriverResult Display(SuggestionsPart part, string displayType, dynamic shapeHelper)
        {

            var suggestions = _userVoiceService.GetSuggestionsList(part.ForumId);

            return ContentShape("Parts_Suggestions", () => shapeHelper.Parts_Suggestions(
                Suggestions: suggestions ));
        }

        protected override DriverResult Editor(SuggestionsPart part, dynamic shapeHelper)
        {
            List<SelectListItem> forums = GetForumListOrDefault(defaultForumId: part.ForumId);

            var viewModel = new SuggestionsSettingsViewModel
            {
                ForumId = part.ForumId,
                Forums = forums,
            };

            return ContentShape("Parts_Suggestions_Edit",
                        () => shapeHelper.EditorTemplate(
                            TemplateName: "Suggestions",
                            Model: viewModel,
                            Prefix: Prefix));
        }

        private List<SelectListItem> GetForumListOrDefault(string defaultForumId)
        {
            List<SelectListItem> list;

            try
            {
                list = CreateForumListFromUserVoice();
            }
            catch(Exception e)
            {
                var errorMessage = T("Could not retrieve forums from UserVoice");
                LogAndNotifyOnError(e, errorMessage);

                list = CreateForumListFromDefault(defaultForumId);
            }

            return list;
        }

        private void LogAndNotifyOnError(Exception e, LocalizedString errorMessage)
        {
            Logger.Error(e, errorMessage.ToString());
            _orchardServices.Notifier.Error(errorMessage);
        }

        private static List<SelectListItem> CreateForumListFromDefault(string defaultForumId)
        {
            return new List<SelectListItem> { new SelectListItem { Text = "Unknown", Value = defaultForumId } };
        }

        private List<SelectListItem> CreateForumListFromUserVoice()
        {
            var result = _userVoiceService.GetForumsList();

            var list = new List<SelectListItem>();
            foreach (var forum in result.forums)
            {
                list.Add(new SelectListItem
                {
                    Text = forum.name,
                    Value = forum.id.ToString(),
                });
            }
            return list;
        }

        protected override DriverResult Editor(SuggestionsPart part, Orchard.ContentManagement.IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}