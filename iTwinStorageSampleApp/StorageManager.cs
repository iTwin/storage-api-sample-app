/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using ItwinProjectSampleApp;
using iTwinStorageSampleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace iTwinStorageSampleApp
    {
    internal class StorageManager : IAsyncDisposable
        {
        private readonly EndpointManager _endpointMgr;
        private readonly string _projectId;

        private List<Folder> _folders = new(); // Folders that will be deleted in DisposeAsync
        private List<File> _files = new(); // Files that will be deleted in DisposeAsync
        private HashSet<string> _downloadedFilesPaths = new(); // Local files that will be deleted in DisposeAsync

        internal StorageManager(string token, string projectId)
            {
            _endpointMgr = new EndpointManager(token);
            _projectId = projectId;
            }

        public async ValueTask DisposeAsync()
            {
            foreach (var folder in _folders)
                {
                try
                    {
                    await DeleteFolderAsync(folder.Id);
                    await DeleteFolderFromRecycleBinAsync(folder.Id);
                    }
                catch
                    {
                    // Ignore
                    }
                }

            foreach (var file in _files)
                {
                try
                    {
                    await DeleteFileAsync(file.Id);
                    await DeleteFileFromRecycleBinAsync(file.Id);
                    }
                catch
                    {
                    // Ignore
                    }
                }

            foreach (var path in _downloadedFilesPaths)
                {
                DeleteLocalFile(path);
                }
            }

        /// <summary>
        /// Get top level files and folders containing a link to the root folder
        /// </summary>
        /// <param name="skip">The number of skipped items</param>
        /// <param name="top">The number of taken items</param>
        /// <returns>Items details containing files, folders, and links</returns>
        internal async Task<ItemsDetails<TopLevelItemsLinks>> GetTopLevelFilesAndFoldersAsync(int? skip = null, int? top = null)
            {
            Console.Write("\n\n- Getting top level files and folders");

            var paginationFilter = GetPaginationFilter(skip, top);

            var responseMsg = await _endpointMgr.MakeGetCall<TopLevelItemsLinks>($"/storage?projectId={_projectId}{paginationFilter}");
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Folders.Count()} folders and {responseMsg.Instances.Files.Count()} files] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Get folders and files from the folder by specifying its ID
        /// </summary>
        /// <param name="folderId">Folder ID</param>
        /// <param name="skip">The number of skipped items</param>
        /// <param name="top">The number of taken items</param>
        /// <returns>Items details containing files, folders, and links</returns>
        internal async Task<ItemsDetails<PaginationLinks>> GetFoldersAndFilesFromFolderAsync(string folderId, int? skip = null, int? top = null)
            {
            Console.Write("\n\n- Getting folders and files from folder");

            var paginationFilter = GetPaginationFilter(skip, top);
            var filter = paginationFilter == string.Empty ? string.Empty : $"?{paginationFilter}";
            var responseMsg = await _endpointMgr.MakeGetCall<PaginationLinks>($"/storage/folders/{folderId}/list{filter}");
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Folders.Count()} folders and {responseMsg.Instances.Files.Count()} files] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Get a single instance by using URL from the link
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="link">Link to the instance</param>
        /// <returns>Instance</returns>
        internal async Task<T> GetInstanceFromLinkAsync<T>(Link link)
            where T : Item
            {
            Console.Write("\n\n- Getting instance from the link");

            var responseMsg = await _endpointMgr.MakeGetSingleCall<T>(link.Href);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {typeof(T).Name} '{responseMsg.Instance.DisplayName}'] (SUCCESS)");

            return responseMsg.Instance;
            }

        /// <summary>
        /// Get instances by using URL from the link
        /// </summary>
        /// <typeparam name="T">Items' links type</typeparam>
        /// <param name="link">Link to the instances</param>
        /// <returns>Instances</returns>
        internal async Task<ItemsDetails<T>> GetInstancesFromLinkAsync<T>(Link link)
            where T : PaginationLinks
            {
            Console.Write("\n\n- Getting instances from the link");

            var responseMsg = await _endpointMgr.MakeGetCall<T>(link.Href);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Folders.Count()} folders and {responseMsg.Instances.Files.Count()} files] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="fileId">File ID to download</param>
        /// <param name="fullPath">Full path where file should be saved</param>
        /// <returns></returns>
        internal async Task DownloadFileAsync(string fileId, string fullPath)
            {
            Console.Write("\n\n- Downloading a file");

            var responseMsg = await _endpointMgr.MakeDownloadCall($"/storage/files/{fileId}/download", fullPath);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception(responseMsg.Content);

            _downloadedFilesPaths.Add(fullPath);

            Console.Write($" [File with ID {fileId} was downloaded] (SUCCESS)");
            }

        /// <summary>
        /// Create a folder in another folder
        /// </summary>
        /// <param name="folderToCreate">Folder model to create</param>
        /// <param name="parentFolderId">Parent folder ID</param>
        /// <returns>Created folder</returns>
        internal async Task<Folder> CreateFolderAsync(FolderCreate folderToCreate, string parentFolderId)
            {
            Console.Write("\n\n- Creating a folder");

            var responseMsg = await _endpointMgr.MakePostCall<FolderCreate, Folder>($"/storage/folders/{parentFolderId}/folders", folderToCreate);
            if (responseMsg.Status != HttpStatusCode.Created)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Folder '{responseMsg.Instance.DisplayName}' was created] (SUCCESS)");

            _folders.Add(responseMsg.Instance);

            return responseMsg.Instance;
            }

        /// <summary>
        /// Update folder
        /// </summary>
        /// <param name="folderToUpdate">Folder model to update</param>
        /// <param name="folderId">Folder ID</param>
        /// <returns>Updated folder</returns>
        internal async Task<Folder> UpdateFolderAsync(FolderUpdate folderToUpdate, string folderId)
            {
            Console.Write("\n\n- Updating a folder");

            var responseMsg = await _endpointMgr.MakePatchCall<FolderUpdate, Folder>($"/storage/folders/{folderId}", folderToUpdate);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Folder '{responseMsg.UpdatedInstance.DisplayName}' was update] (SUCCESS)");

            return responseMsg.UpdatedInstance;
            }

        /// <summary>
        /// Update file
        /// </summary>
        /// <param name="fileToUpdate">File model to update</param>
        /// <param name="fileId">Folder ID</param>
        /// <returns>Updated file</returns>
        internal async Task<File> UpdateFileAsync(FileUpdate fileToUpdate, string fileId)
            {
            Console.Write("\n\n- Updating a file");

            var responseMsg = await _endpointMgr.MakePatchCall<FileUpdate, File>($"/storage/files/{fileId}", fileToUpdate);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [File '{responseMsg.UpdatedInstance.DisplayName}' was update] (SUCCESS)");

            return responseMsg.UpdatedInstance;
            }

        /// <summary>
        /// Create a file in folder
        /// </summary>
        /// <param name="fileToCreate">File model to create</param>
        /// <param name="parentFolderId">Parent folder ID</param>
        /// <param name="content">File's content</param>
        /// <returns>Created file</returns>
        internal async Task<File> CreateFileAsync(FileCreate fileToCreate, string parentFolderId, System.IO.Stream content)
            {
            Console.Write("\n\n- Creating a file");

            Console.Write("\n\t- Creating file's metadata");

            var createResponse = await _endpointMgr.MakePostCall<FileCreate, FileUploadLinks>($"/storage/folders/{parentFolderId}/files", fileToCreate);
            if (createResponse.Status != HttpStatusCode.Accepted)
                throw new Exception($"{createResponse.Status}: {createResponse.ErrorDetails?.Code} - {createResponse.ErrorDetails?.Message}");

            Console.Write($" [File's metadata was created] (SUCCESS)");

            Console.Write("\n\t- Uploading a file");
            var uploadResponse = await _endpointMgr.MakeFileUploadCall(createResponse.Instance.UploadUrl.Href, content);
            if (uploadResponse.Status != HttpStatusCode.Created)
                throw new Exception(uploadResponse.Content);
            Console.Write($" [File was uploaded] (SUCCESS)");

            Console.Write("\n\t- Confirming file's creation");
            var confirmRespnse = await _endpointMgr.MakePostCall<File>(createResponse.Instance.CompleteUrl.Href);
            if (confirmRespnse.Status != HttpStatusCode.OK)
                throw new Exception($"{confirmRespnse.Status}: {confirmRespnse.ErrorDetails?.Code} - {confirmRespnse.ErrorDetails?.Message}");
            _files.Add(confirmRespnse.Instance);

            Console.Write($" [File '{confirmRespnse.Instance.DisplayName}' was created] (SUCCESS)");

            return confirmRespnse.Instance;
            }

        /// <summary>
        /// Update file's content
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <param name="content">File's content</param>
        /// <returns>Updated file</returns>
        internal async Task<File> UpdateFileContentAsync(string fileId, System.IO.Stream content)
            {
            Console.Write("\n\n- Updating file's content");

            Console.Write("\n\t- Creating update links");

            var createResponse = await _endpointMgr.MakePostCall<FileUploadLinks>($"/storage/files/{fileId}/updateContent ");
            if (createResponse.Status != HttpStatusCode.Accepted)
                throw new Exception($"{createResponse.Status}: {createResponse.ErrorDetails?.Code} - {createResponse.ErrorDetails?.Message}");

            Console.Write($" [File's upload links created] (SUCCESS)");

            Console.Write("\n\t- Uploading a file");
            var uploadResponse = await _endpointMgr.MakeFileUploadCall(createResponse.Instance.UploadUrl.Href, content);
            if (uploadResponse.Status != HttpStatusCode.Created)
                throw new Exception(uploadResponse.Content);
            Console.Write($" [File was uploaded] (SUCCESS)");

            Console.Write("\n\t- Confirming file's update");
            var confirmRespnse = await _endpointMgr.MakePostCall<File>(createResponse.Instance.CompleteUrl.Href);
            if (confirmRespnse.Status != HttpStatusCode.OK)
                throw new Exception($"{confirmRespnse.Status}: {confirmRespnse.ErrorDetails?.Code} - {confirmRespnse.ErrorDetails?.Message}");

            Console.Write($" [File '{confirmRespnse.Instance.DisplayName}' was created] (SUCCESS)");

            return confirmRespnse.Instance;
            }

        /// <summary>
        /// Delete a folder
        /// </summary>
        /// <param name="folderId">Folder ID</param>
        /// <returns></returns>
        internal async Task DeleteFolderAsync(string folderId)
            {
            Console.Write("\n\n- Deleting a folder");

            var responseMsg = await _endpointMgr.MakeDeleteCall($"/storage/folders/{folderId}");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Folder with ID {folderId} was deleted] (SUCCESS)");
            }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="fileId">File ID</param>
        /// <returns></returns>
        internal async Task DeleteFileAsync(string fileId)
            {
            Console.Write("\n\n- Deleting a file");

            var responseMsg = await _endpointMgr.MakeDeleteCall($"/storage/files/{fileId}");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [File with ID {fileId} was deleted] (SUCCESS)");
            }

        /// <summary>
        /// Delete a folder from the recycle bin
        /// </summary>
        /// <param name="folderId">Folder ID in the recycle bin</param>
        /// <returns></returns>
        internal async Task DeleteFolderFromRecycleBinAsync(string folderId)
            {
            Console.Write("\n\n- Deleting a folder from recycle bin");

            var responseMsg = await _endpointMgr.MakeDeleteCall($"/storage/recycleBin/folders/{folderId}");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Folder with ID {folderId} was deleted from recycle bin] (SUCCESS)");
            }

        /// <summary>
        /// Delete a file from the recycle bin
        /// </summary>
        /// <param name="fileId">File ID in the recycle bin</param>
        /// <returns></returns>
        internal async Task DeleteFileFromRecycleBinAsync(string fileId)
            {
            Console.Write("\n\n- Deleting a file  from recycle bin");

            var responseMsg = await _endpointMgr.MakeDeleteCall($"/storage/recycleBin/files/{fileId}");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [File with ID {fileId} was deleted from recycle bin] (SUCCESS)");
            }

        /// <summary>
        /// Get folders and files in the recycle bin
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="skip">The number of skipped items</param>
        /// <param name="top">The number of taken items</param>
        /// <returns>Items details containing files, folders, and links from the recycle bin</returns>
        internal async Task<ItemsDetails<PaginationLinks>> GetFoldersAndFilesInRecycleBinAsync(string projectId, int? skip = null, int? top = null)
            {
            Console.Write("\n\n- Getting folders and files in recycle bin");

            var paginationFilter = GetPaginationFilter(skip, top);
            var responseMsg = await _endpointMgr.MakeGetCall<PaginationLinks>($"/storage/recycleBin?projectId={projectId}{paginationFilter}");
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Folders.Count()} folders and {responseMsg.Instances.Files.Count()} files] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Restore a folder
        /// </summary>
        /// <param name="folderId">Folder ID from recycle bin</param>
        /// <returns></returns>
        internal async Task RestoreFolderAsync(string folderId)
            {
            Console.Write("\n\n- Restoring a folder");

            var responseMsg = await _endpointMgr.MakePostCall($"/storage/recycleBin/folders/{folderId}/restore");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Folder with ID {folderId} was restored] (SUCCESS)");
            }

        /// <summary>
        /// Restore a file
        /// </summary>
        /// <param name="fileId">File ID from recycle bin</param>
        /// <returns></returns>
        internal async Task RestoreFileAsync(string fileId)
            {
            Console.Write("\n\n- Restoring a file");

            var responseMsg = await _endpointMgr.MakePostCall($"/storage/recycleBin/files/{fileId}/restore");
            if (responseMsg.Status != HttpStatusCode.NoContent)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [File with ID {fileId} was restored] (SUCCESS)");
            }

        private static string GetPaginationFilter(int? skip, int? top)
            {
            var paginationFilter = string.Empty;
            if (skip != null)
                paginationFilter += $"&$skip={skip}";
            if (top != null)
                paginationFilter += $"&$top={top}";
            return paginationFilter;
            }

        private static void DeleteLocalFile(string fullPath)
            {
            Console.Write("\n\n- Deleting local file");

            if (System.IO.File.Exists(fullPath))
                {
                System.IO.File.Delete(fullPath);
                Console.Write($" [File from '{fullPath}' was deleted] (SUCCESS)");
                }
            }
        }
    }
