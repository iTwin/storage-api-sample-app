/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace iTwinStorageSampleApp.Models
    {
    public class TopLevelItems : ItemsBase
        {
        [JsonPropertyName("_links")]
        public TopLevelItemsLinks Links { get; set; }
        }
    }
