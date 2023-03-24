# Storage API Sample App

Copyright © Bentley Systems, Incorporated. All rights reserved.

An iTwin sample application that demonstrates how to create, query and update files with folders saved in the Storage.

This application contains sample code that should not be used in a production environment. It contains no retry logic and minimal logging/error handling.

## Prerequisites

- [Git](https://git-scm.com/)
- Visual Studio 2019 or [Visual Studio Code](https://code.visualstudio.com/)
- [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0/)

## Development Setup (Visual Studio 2019)

1. Clone Repository

2. Open iTwinStorageSampleApp.sln and Build

3. (Optional) Put breakpoint in Program.cs

4. Run to debug

5. It will require a user token and project ID.

   - User token retrieval:
      - Go to the Storage API [developer portal](https://developer.bentley.com/apis/storage/operations/get-top-level-folders-and-files-by-project/)
      - Click the Try it out Button
      - In the popup window, select authorizationCode in the Bentley OAuth2 Service dropdown
      - This will popup another window that will require you to login.
      - After you login, the Authorization header will be populated. Copy the entire string and paste into the command window for the iTwin Sample Project App.
      - Press Enter

   - Project ID retrieval:
      - Use existing project ID or follow [Create & Query Projects](https://developer.bentley.com/tutorials/create-and-query-projects-guide/) tutorial to create a new project or retrieve existing one.

6. You can now step through the code that will create and manage files and folders.
