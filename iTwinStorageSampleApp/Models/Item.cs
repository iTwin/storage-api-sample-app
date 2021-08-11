/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using Newtonsoft.Json;
using System;

namespace iTwinStorageSampleApp.Models
    {
    public class Item
        {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string LastModifiedByDisplayName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string ParentFolderId { get; set; }
        [JsonProperty ("_links")]
        public ItemLinks Links { get; set; }
        }
    }
