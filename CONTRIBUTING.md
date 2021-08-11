# Contributing

We welcome all types of contributions.

Found a bug? Please create an [issue](https://github.com/iTwin/storage-api-sample-app/issues).

Want to contribute by creating a pull request? Great! [Fork this repository](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/working-with-forks) to get started.

---

## How to setup

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
      - Go to the Storage API [developer portal](https://developer.bentley.com/api-groups/data-management/apis/storage/operations/get-top-level-folders-and-files-by-project/)
      - Click the TryIt Button
      - In the popup window, select authorizationCode in the Bentley OAuth2 Service dropdown
      - This will popup another window that will require you to login.
      - After you login, the Authorization header will be populated. Copy the entire string and paste into the command window for the iTwin Sample Project App.
      - Press Enter

   - Project ID retrieval:
      - Use existing project ID or follow [Create & Query Projects](https://developer.bentley.com/tutorials/create-and-query-projects-guide/) tutorial to create a new project or retrieve existing one.

6. You can now step through the code that will create and manage files and folders.

---

## Pull Requests

Before creating a pull request, make sure your changes address a specific issue. Do a search to see if there are any existing issues that are still open. If you don't find one, you can create one.

To enable us to quickly review and accept your pull requests, always create one pull request per issue. Never merge multiple requests in one unless they have the same root cause. Be sure to follow best practices and keep code changes as small as possible. Avoid pure formatting changes or random "fixes" that are unrelated to the linked issue.

---

## Contributor License Agreement (CLA)

You will be asked to sign a Contribution License Agreement with Bentley before your contributions will be accepted.
This a one-time requirement for Bentley projects in GitHub.
You can read more about [Contributor License Agreements](https://en.wikipedia.org/wiki/Contributor_License_Agreement) on Wikipedia.

> Note: a CLA is not required if the change is trivial (such as fixing a spelling error or a typo).
