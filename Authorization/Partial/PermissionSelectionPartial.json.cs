using System;
using System.Collections.Generic;
using System.Linq;
using Simplified.Ring2;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Database;
using Starcounter.Authorization.Helper;

namespace Starcounter.Authorization.Partial
{
    public partial class PermissionSelectionPartial : Json
    {
        public EventHandler<PermissionSelectionPartialEventArgs> MemberAdded;
        public EventHandler<PermissionSelectionPartialEventArgs> MemberRemoved;
        private Func<PermissionToken> _getOrCreateToken;
        private PermissionToken _permissionToken;
        private Permission _permission;

        public static void RegisterResources()
        {
            Starcounter.Handle.GET("/Authorization/viewmodels/PermissionSelectionPartial.html", () =>
            {
                var assembly = typeof(PermissionSelectionPartial).Assembly;
                return new Response
                {
                    StreamedBody = assembly.GetManifestResourceStream(
                        "Starcounter.Authorization.Partial.PermissionSelectionPartial.html"),
                    ContentType = "text/html"
                };
            });
        }

        public static PermissionSelectionPartial Create<TPermission>(TPermission permission)
            where TPermission : Permission
        {
            var permissionSelectionPartial = new PermissionSelectionPartial();
            permissionSelectionPartial.Init(permission);
            return permissionSelectionPartial;
        }

        public void Init<TPermission>(TPermission permission) where TPermission : Permission
        {
            _permission = permission;
            _getOrCreateToken =
                () => _permissionToken ?? (_permissionToken = PermissionToken.GetForPermissionOrCreate(permission));
            _permissionToken = PermissionToken.GetForPermissionOrNull(permission);
            ReloadMembers();
        }

        public void ReloadMembers()
        {
            _permissionToken = PermissionToken.GetForPermissionOrNull(_permission);
            if (_permissionToken != null)
            {
                CurrentMembers.Data =
                    PermissionHelper.GetAllPsgForPermission(_permissionToken);
                foreach (var currentMemberItem in CurrentMembers)
                {
                    currentMemberItem.RemoveAction = RemoveAssociation;
                }
            }
            else
            {
                CurrentMembers.Clear();
            }
        }

        private void AssociateMemberWithPermission(GroupResultItem item)
        {
            new PermissionSomebodyGroup
            {
                Permission = _getOrCreateToken(),
                Group = item.Data
            };
            ReloadMembers();
            Query = string.Empty;
            RefreshSearchResultList(Query);
            MemberAdded?.Invoke(this, new PermissionSelectionPartialEventArgs(item.Data));
        }

        private void RemoveAssociation(PermissionSomebodyGroup psg)
        {
            var group = psg.Group;
            psg.Delete();
            if (!PermissionHelper.GetAllPsgForPermission(_permissionToken).Any())
            {
                _permissionToken.Delete();
                _permissionToken = null;
            }
            ReloadMembers();
            MemberRemoved?.Invoke(this, new PermissionSelectionPartialEventArgs(group));
        }

        private void Handle(Input.Query action)
        {
            RefreshSearchResultList(action.Value);
        }

        private void RefreshSearchResultList(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                Results.Data = null;
                return;
            }

            Results.Data = SearchForSomebodyGroups(search);

            foreach (var item in Results)
            {
                item.SelectResult = AssociateMemberWithPermission;
            }
        }

        private IEnumerable<GroupResultItem> SearchForSomebodyGroups(string nameToLookFor)
        {
            var currentGroups = CurrentMembers.Select(memberItem => memberItem.Data.Group).ToList();
            return Db.SQL<SomebodyGroup>(
                $"SELECT s FROM {typeof(SomebodyGroup).FullName} s WHERE s.{nameof(SomebodyGroup.Name)} LIKE ?",
                $"%{nameToLookFor.ToLowerInvariant()}%")
                .Except(currentGroups)
                .Select(item => new GroupResultItem {Key = item.Key, Data = item});
        }

        [PermissionSelectionPartial_json.CurrentMembers]
        partial class CurrentMemberItem : Json, IBound<PermissionSomebodyGroup>
        {
            public Action<PermissionSomebodyGroup> RemoveAction { get; set; }

            public string Name => Data.Group.Name;

            private void Handle(Input.Remove action)
            {
                RemoveAction?.Invoke(Data);
            }
        }

        [PermissionSelectionPartial_json.Results]
        partial class GroupResultItem : Json, IBound<SomebodyGroup>
        {
            public Action<GroupResultItem> SelectResult { get; set; }

            public void Handle(Input.Select action)
            {
                SelectResult.Invoke(this);
            }
        }
    }
}