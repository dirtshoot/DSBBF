using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Szmyd.Orchard.Modules.Sharing
{
    [UsedImplicitly]
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission EditSettings = new Permission { Description = "Edit AddThis settings", Name = "EditAddThisSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                EditSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {EditSettings}
                }
            };
        }

    }
}
