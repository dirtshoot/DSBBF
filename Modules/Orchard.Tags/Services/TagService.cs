﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tags.Models;
using Orchard.UI.Notify;

namespace Orchard.Tags.Services {
    [UsedImplicitly]
    public class TagService : ITagService {
        private readonly IRepository<TagRecord> _tagRepository;
        private readonly IRepository<ContentTagRecord> _contentTagRepository;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrchardServices _orchardServices;

        public TagService(IRepository<TagRecord> tagRepository,
                          IRepository<ContentTagRecord> contentTagRepository,
                          INotifier notifier,
                          IAuthorizationService authorizationService,
                          IOrchardServices orchardServices) {
            _tagRepository = tagRepository;
            _contentTagRepository = contentTagRepository;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<TagRecord> GetTags() {
            return _tagRepository.Table.ToList();
        }

        public TagRecord GetTag(int tagId) {
            return _tagRepository.Get(x => x.Id == tagId);
        }

        public TagRecord GetTagByName(string tagName) {
            return _tagRepository.Get(x => x.TagName == tagName);
        }

        public TagRecord CreateTag(string tagName) {
            var result = _tagRepository.Get(x => x.TagName == tagName);
            if (result == null) {
                result = new TagRecord { TagName = tagName };
                _tagRepository.Create(result);
            }

            return result;
        }

        public void DeleteTag(int tagId) {
            _authorizationService.CheckAccess(Permissions.ManageTags, _orchardServices.WorkContext.CurrentUser, null);

            var tag = GetTag(tagId);
            if (tag == null)
                return;

            // Delete associations to content items
            foreach (var tagContentItem in _contentTagRepository.Fetch(x => x.TagRecord == tag)) {
                _contentTagRepository.Delete(tagContentItem);
            }

            // Delete tag entity
            _tagRepository.Delete(tag);
        }

        public void UpdateTag(int tagId, string tagName) {
            _authorizationService.CheckAccess(Permissions.ManageTags, _orchardServices.WorkContext.CurrentUser, null);

            if (String.IsNullOrEmpty(tagName)) {
                _notifier.Warning(T("Couldn't rename tag: name was empty"));
                return;
            }

            var tagRecord = GetTagByName(tagName);

            // new tag name already existing => merge
            if (tagRecord != null) {
                // If updating ourselves, ignore
                if (tagRecord.Id == tagId)
                    return;

                var tagsContentItems = _contentTagRepository.Fetch(x => x.TagRecord.Id == tagId);

                // get contentItems already tagged with the existing one
                var taggedContentItems = GetTaggedContentItems(tagRecord.Id);

                foreach (var tagContentItem in tagsContentItems) {
                    ContentTagRecord item = tagContentItem;
                    if (!taggedContentItems.Any(c => c.ContentItem.Id == item.TagsPartRecord.Id)) {
                        TagContentItem(tagContentItem.TagsPartRecord, tagName);
                    }
                    _contentTagRepository.Delete(tagContentItem);
                }

                _tagRepository.Delete(GetTag(tagId));
                return;
            }

            // Create new tag
            tagRecord = _tagRepository.Get(tagId);
            tagRecord.TagName = tagName;
        }

        public IEnumerable<IContent> GetTaggedContentItems(int tagId) {
            return GetTaggedContentItems(tagId, VersionOptions.Published);
        }

        public IEnumerable<IContent> GetTaggedContentItems(int tagId, VersionOptions options) {
            return _contentTagRepository
                .Fetch(x => x.TagRecord.Id == tagId)
                .Select(t => _orchardServices.ContentManager.Get(t.TagsPartRecord.Id, options))
                .Where(c => c != null);
        }

        public IEnumerable<IContent> GetTaggedContentItems(int tagId, int skip, int take) {
            return GetTaggedContentItems(tagId, skip, take, VersionOptions.Published);
        }

        public IEnumerable<IContent> GetTaggedContentItems(int tagId, int skip, int take, VersionOptions options) {
            return _orchardServices.ContentManager
                .Query<TagsPart, TagsPartRecord>()
                .Where(tpr => tpr.Tags.Any(tr => tr.TagRecord.Id == tagId))
                .Join<CommonPartRecord>().OrderByDescending(x => x.CreatedUtc)
                .Slice(skip, take);
        }

        public int GetTaggedContentItemCount(int tagId) {
            return GetTaggedContentItemCount(tagId, VersionOptions.Published);
        }

        public int GetTaggedContentItemCount(int tagId, VersionOptions options) {
            return GetTaggedContentItems(tagId, options).Count();
        }

        private void TagContentItem(TagsPartRecord tagsPartRecord, string tagName) {
            var tagRecord = GetTagByName(tagName);
            var tagsContentItems = new ContentTagRecord { TagsPartRecord = tagsPartRecord, TagRecord = tagRecord };
            _contentTagRepository.Create(tagsContentItems);
        }

        public void RemoveTagsForContentItem(ContentItem contentItem) {
            if (contentItem.Id == 0)
                throw new OrchardException(T("Error removing tag to content item: the content item has not been created yet."));

            _contentTagRepository.Flush();

            var tagsPart = contentItem.As<TagsPart>();

            // delete orphan tags (for each tag, if there is no other contentItem than the one being deleted, it's an orphan)
            foreach (var tag in tagsPart.CurrentTags) {
                if (_contentTagRepository.Count(x => x.TagsPartRecord.Id != contentItem.Id) == 0) {
                    _tagRepository.Delete(tag);
                }
            }

            // delete tag links with this contentItem (ContentTagRecords)
            foreach (var record in _contentTagRepository.Fetch(x => x.TagsPartRecord.Id == contentItem.Id)) {
                _contentTagRepository.Delete(record);
            }
        }

        public void UpdateTagsForContentItem(ContentItem contentItem, IEnumerable<string> tagNamesForContentItem) {
            if (contentItem.Id == 0)
                throw new OrchardException(T("Error adding tag to content item: the content item has not been created yet."));

            var tags = tagNamesForContentItem.Select(CreateTag);
            var newTagsForContentItem = new List<TagRecord>(tags);
            var currentTagsForContentItem = _contentTagRepository.Fetch(x => x.TagsPartRecord.Id == contentItem.Id);

            foreach (var tagContentItem in currentTagsForContentItem) {
                if (!newTagsForContentItem.Contains(tagContentItem.TagRecord)) {
                    _contentTagRepository.Delete(tagContentItem);
                }
                else {
                    newTagsForContentItem.Remove(tagContentItem.TagRecord);
                }
            }

            foreach (var newTagForContentItem in newTagsForContentItem) {
                _contentTagRepository.Create(new ContentTagRecord { TagsPartRecord = contentItem.As<TagsPart>().Record, TagRecord = newTagForContentItem });
            }
        }
    }
}
