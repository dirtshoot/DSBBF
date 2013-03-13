using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using UserVoice.Widgets.Models;
using UserVoice.Widgets.Services;
using UserVoice.Widgets.ViewModels;
using Orchard.Logging;

namespace UserVoice.Widgets.Drivers
{
    public class FeedbackPartDriver : ContentPartDriver<FeedbackPart>
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserVoiceService _userVoiceService;
        public FeedbackPartDriver(IOrchardServices services,
                                  IUserVoiceService userVoiceService)
        {
            _orchardServices = services;
            _userVoiceService = userVoiceService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override DriverResult Display(FeedbackPart part, string displayType, dynamic shapeHelper)
        {
            var settings = _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>();

            return ContentShape("Parts_Feedback", () => shapeHelper.Parts_Feedback(
                Feedback: new FeedbackViewModel
                {
                    Account = settings.Account,
                    Host = settings.Host,
                    TabLabel = part.TabLabel,
                    TabPosition = GetTabPositionString(part.TabPosition),
                    TabInverted = part.TabInverted,
                    TabEnabled = part.TabEnabled,
                    TabColor = part.TabColor,
                    WidgetId = part.WidgetId,
                    SsoToken = null
                }));
        }

        private static string GetTabPositionString(TabPosition tabPosition)
        {
            string result = null;

            if (tabPosition == TabPosition.BottomLeft)
                result = "bottom-left";
            if (tabPosition == TabPosition.BottomRight)
                result = "bottom-right";

            return result;
        }

        protected override DriverResult Editor(FeedbackPart part, dynamic shapeHelper)
        {
            var settings = _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>();

            if (string.IsNullOrEmpty(settings.Account)
                || string.IsNullOrEmpty(settings.ApiKey)
                || string.IsNullOrEmpty(settings.Host))
            {
                this._orchardServices.Notifier.Error(this.T("UserVoice is not configured."));
            }

            var tabPositions = GetTabPositionsList();
            List<SelectListItem> widgets = GetWidgetsListOrDefault(defaultForumId: part.WidgetId);

            var viewModel = new FeedbackSettingsViewModel
            {
                Account = settings.Account,

                TabLabel = part.TabLabel,
                TabColor = part.TabColor,
                TabPosition = Enum.GetName(typeof(TabPosition), part.TabPosition),
                TabInverted = part.TabInverted,
                TabEnabled = part.TabEnabled,
                TabPositions = tabPositions,
                WidgetId = part.WidgetId,
                Widgets = widgets
            };

            return ContentShape("Parts_Feedback_Edit",
                        () => shapeHelper.EditorTemplate(
                            TemplateName: "Feedback",
                            Model: viewModel,
                            Prefix: Prefix));
        }

        private List<SelectListItem> GetWidgetsListOrDefault(string defaultForumId)
        {
            List<SelectListItem> list;

            try
            {
                list = CreateWidgetsListFromUserVoice();
            }
            catch (Exception e)
            {
                var errorMessage = T("Could not retrieve widgets list from UserVoice");
                LogAndNotifyOnError(e, errorMessage);

                list = CreateWidgetsListFromDefault(defaultForumId);
            }

            return list;

        }

        private List<SelectListItem> CreateWidgetsListFromUserVoice()
        {
            var result = _userVoiceService.GetClientsList();

            var list = new List<SelectListItem>();
            foreach (var client in result.client)
            {
                if (client.type == "widget")
                {
                    list.Add(new SelectListItem
                    {
                        Text = client.name,
                        Value = client.key,
                    });
                }
            }
            return list;
        }
        
        private List<SelectListItem> CreateWidgetsListFromDefault(string defaultForumId)
        {
            return new List<SelectListItem> { new SelectListItem { Text = "Unknown", Value = defaultForumId } };
        }

        private void LogAndNotifyOnError(Exception e, LocalizedString errorMessage)
        {
            Logger.Error(e, errorMessage.ToString());
            _orchardServices.Notifier.Error(errorMessage);
        }



        private static List<SelectListItem> GetTabPositionsList()
        {
            var positionNames = Enum.GetNames(typeof(TabPosition));
            var tabPositions = positionNames.Select(name => new SelectListItem { Text = name, Value = name }).ToList();
            return tabPositions;
        }

        protected override DriverResult Editor(FeedbackPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}