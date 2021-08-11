/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace iTwinStorageSampleApp.Models
    {
    public class ItemsDetails<T>
        where T : PaginationLinks
        {
        public IEnumerable<File> Files
            {
            get; set;
            }

        public IEnumerable<Folder> Folders
            {
            get; set;
            }
        public T Links
            {
            get; set;
            }
        }
    }
