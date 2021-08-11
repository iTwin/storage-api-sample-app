/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace iTwinStorageSampleApp.Models
    {
    public class Items : ItemsBase
        {
        [JsonPropertyName("_links")]
        public PaginationLinks Links { get; set; }
        }
    }
