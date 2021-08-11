/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

namespace iTwinStorageSampleApp.Models
    {
    public class FileCreate
        {
        public string DisplayName { get; }
        public string Description { get; }

        public FileCreate(string displayName)
            {
            DisplayName = displayName;
            }

        public FileCreate(string displayName, string description)
            : this(description)
            {
            DisplayName = displayName;
            }
        }
    }
