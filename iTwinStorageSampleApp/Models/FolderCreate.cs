/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

namespace iTwinStorageSampleApp.Models
    {
    public class FolderCreate
        {
        public string DisplayName { get; }
        public string Description { get; }

        public FolderCreate(string displayName)
            {
            DisplayName = displayName;
            }

        public FolderCreate(string displayName, string description)
            : this(description)
            {
            DisplayName = displayName;
            }
        }
    }
