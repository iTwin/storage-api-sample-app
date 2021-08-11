/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace iTwinStorageSampleApp.Models
    {
    public class ItemsBase
        {
        public IEnumerable<JObject> Items { get; set; }
        }
    }
