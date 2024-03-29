﻿using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Email.Models;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Email.Services {
    public class MissingSettingsBanner: INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {

            var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

            if ( smtpSettings == null || !smtpSettings.IsValid() ) {
                yield return new NotifyEntry { Message = T("The SMTP settings needs to be configured." ), Type = NotifyType.Warning};
            }
        }
    }
}
