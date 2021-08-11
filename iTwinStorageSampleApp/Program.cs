/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using iTwinStorageSampleApp.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace iTwinStorageSampleApp
    {
    class Program
        {
        static async Task Main(string[] args)
            {
            DisplayMainIndex();

            // Retrieve the token using the TryIt button. https://developer.bentley.com/api-groups/data-management/apis/storage/operations/get-top-level-folders-and-files-by-project/
            Console.WriteLine("\n\nCopy and paste the Authorization header from the 'Try It' sample in the APIM front-end:  ");
            var authorizationHeader = Console.ReadLine();
            Console.WriteLine("\n\nInsert project Id:   ");
            var projectId = Console.ReadLine();
            Console.Clear();

            DisplayMainIndex();

            await ExecuteStorageWorkflowAsync(authorizationHeader, projectId);
            }

        #region Private Methods

        // This workflow follows Storage API quick start tutorial. https://developer.bentley.com/tutorials/storage-quick-start/
        private static async Task ExecuteStorageWorkflowAsync(string authorizationHeader, string projectId)
            {
            await using var storageMgr = new StorageManager(authorizationHeader, projectId);
            // Let's retrieve top level items. It will contain a link to the root folder
            var topLevelItemsResponse = await storageMgr.GetTopLevelFilesAndFoldersAsync();

            // Getting root folder from the link
            var rootFolder = await storageMgr.GetInstanceFromLinkAsync<Folder>(topLevelItemsResponse.Links.Folder);

            // Creating a folder in the root folder 
            var folder = await storageMgr.CreateFolderAsync(new($"Test Folder - {Guid.NewGuid()}"), rootFolder.Id);

            // Creating a file in the root folder 
            var file = await storageMgr.CreateFileAsync(new($"Test File - {Guid.NewGuid()}.txt"), rootFolder.Id,
                new MemoryStream(Encoding.UTF8.GetBytes("test content")));

            // Download created file
            await storageMgr.DownloadFileAsync(file.Id,
                $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{file.DisplayName}");

            // Getting files and folder from folder
            var items = await storageMgr.GetFoldersAndFilesFromFolderAsync(rootFolder.Id);

            // Updating existing folder
            var folderToUpdate = new FolderUpdate { DisplayName = $"Test Folder update - {Guid.NewGuid()}", Description = "Updated description" };
            var updatedFolder = await storageMgr.UpdateFolderAsync(folderToUpdate, folder.Id);

            // Updating existing file
            var fileToUpdate = new FileUpdate { DisplayName = $"Test file update - {Guid.NewGuid()}.txt", Description = "Updated description" };
            var updatedFile = await storageMgr.UpdateFileAsync(fileToUpdate, file.Id);

            // Updating actual file's content
            await storageMgr.UpdateFileContentAsync(file.Id, new MemoryStream(Encoding.UTF8.GetBytes("test content update")));

            // Deleting a folder
            await storageMgr.DeleteFolderAsync(folder.Id);

            // Deleting a file
            await storageMgr.DeleteFileAsync(file.Id);

            // Getting folders and files in recycle bin
            var recycleBinItems = await storageMgr.GetFoldersAndFilesInRecycleBinAsync(projectId);

            // Restoring a folder
            await storageMgr.RestoreFolderAsync(folder.Id);

            // Restoring a file
            await storageMgr.RestoreFileAsync(file.Id);
            }

        private static void DisplayMainIndex()
            {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.WriteLine("*****************************************************************************************");
            Console.WriteLine("*           iTwin Platform Storage App                                                  *");
            Console.WriteLine("*****************************************************************************************\n");
            }
        #endregion
        }
    }
