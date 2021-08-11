/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using ItwinProjectSampleApp.Models;
using iTwinStorageSampleApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ItwinProjectSampleApp
    {
    internal class EndpointManager
        {
        private static readonly HttpClient storageClient = new();
        private static readonly HttpClient client = new();
        private readonly string _token;
        private const string API_BASE_URL = "https://api.bentley.com";

        #region Constructors
        internal EndpointManager(string token)
            {
            _token = token;
            storageClient.DefaultRequestHeaders.Clear();
            storageClient.DefaultRequestHeaders.Add("Accept", "application/vnd.bentley.itwin-platform.v1+json");
            storageClient.DefaultRequestHeaders.Add("Authorization", token);
            }
        #endregion

        #region GET
        internal async Task<HttpGetFoldersAndFilesResponseMessage<T>> MakeGetCall<T>(string url, Dictionary<string, string> customHeaders = null)
            where T : PaginationLinks
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            using var response = await storageClient.GetAsync(GetFullUrl(url));

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpGetResponseMessage.
            var responseMsg = new HttpGetFoldersAndFilesResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if (response.StatusCode == HttpStatusCode.OK)
                {
                // Successful response. Deserialize the list of objects returned.
                var items = responsePayload.ToObject<ItemsBase>();
                var files = new List<File>();
                var folders = new List<Folder>();
                foreach (var item in items.Items)
                    {
                    if (item.ContainsKey("type") && item.Value<string>("type") == "file")
                        {
                        files.Add(item.ToObject<File>());
                        }
                    else if (item.ContainsKey("type") && item.Value<string>("type") == "folder")
                        {
                        folders.Add(item.ToObject<Folder>());
                        }
                    }
                responseMsg.Instances = new ItemsDetails<T>
                    {
                    Links = responsePayload["_links"].ToObject<T>(),
                    Files = files,
                    Folders = folders
                    };
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpGetSingleResponseMessage<T>> MakeGetSingleCall<T>(string url, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);
 
            using var response = await storageClient.GetAsync(GetFullUrl(url));

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpGetSingleResponseMessage.
            HttpGetSingleResponseMessage<T> responseMsg = new HttpGetSingleResponseMessage<T>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if (response.StatusCode == HttpStatusCode.OK)
                {
                // Successful response. Deserialize the object returned.
                var containerName = typeof(T).Name.ToLower();
                responseMsg.Instance = responsePayload[containerName].ToObject<T>();
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpResponseMessage> MakeDownloadCall(string url, string path, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            var request = new HttpRequestMessage(HttpMethod.Get, GetFullUrl(url));
            request.Headers.Add("Accept", "application/vnd.bentley.itwin-platform.v1+octet-stream");
            request.Headers.Add("Authorization", _token);

            using var response = await storageClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpGetResponseMessage.
            var responseMsg = new HttpResponseMessage();
            responseMsg.Status = response.StatusCode;

            if (response.StatusCode == HttpStatusCode.OK)
                {
                // Copy response to file
                using var fileStream = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
                await response.Content.CopyToAsync(fileStream);
                }
            else
                {
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                }

            return responseMsg;
            }
        #endregion

        #region POST
        internal async Task<HttpPostResponseMessage<TOut>> MakePostCall<TIn, TOut>(string url, TIn propertyModel, Dictionary<string, string> customHeaders = null)
            where TIn : class
            where TOut : class
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            var body = new StringContent(JsonSerializer.Serialize(propertyModel, JsonSerializerOptions), Encoding.UTF8, "application/json");
            var responseMsg = new HttpPostResponseMessage<TOut>();

            using var response = await storageClient.PostAsync(GetFullUrl(url), body);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseMsg.Content))
                {
                var responsePayload = JObject.Parse(responseMsg.Content);
                if (response.IsSuccessStatusCode)
                    {
                    // Successful response. Deserialize the object returned. This is the full representation
                    // of the new instance that was just created. It will contain the new instance Id.
                    responseMsg.Instance = responsePayload.First.First.ToObject<TOut>();
                    }
                else
                    {
                    // There was an error. Deserialize the error details and return.
                    responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                    }
                }
            else
                responseMsg.Instance = null;


            return responseMsg;
            }

        internal async Task<HttpPostResponseMessage<T>> MakePostCall<T>(string url, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            var responseMsg = new HttpPostResponseMessage<T>();

            using var response = await storageClient.PostAsync(GetFullUrl(url), null);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            if (response.IsSuccessStatusCode)
                {
                if (!string.IsNullOrEmpty(responseMsg.Content))
                    {
                    var responsePayload = JObject.Parse(responseMsg.Content);
                    // Successful response. Deserialize the object returned. This is the full representation
                    // of the new instance that was just created. It will contain the new instance Id.
                    responseMsg.Instance = responsePayload.First.First.ToObject<T>();
                    }
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                var responsePayload = JObject.Parse(responseMsg.Content);
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }

        internal async Task<HttpResponseMessage> MakePostCall(string url, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            var responseMsg = new HttpResponseMessage();

            using var response = await storageClient.PostAsync(GetFullUrl(url), null);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();

            // Copy/Deserialize the response into custom HttpPostResponseMessage.
            if (!response.IsSuccessStatusCode)
                {
                // There was an error. Deserialize the error details and return.
                var responsePayload = JObject.Parse(responseMsg.Content);
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }
        #endregion

        #region PATCH
        internal async Task<HttpPatchResponseMessage<TOut>> MakePatchCall<TIn, TOut>(string url, TIn patchedObject, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            // Construct full url and then make the PATCH call
            using var response = await storageClient.PatchAsync(GetFullUrl(url),
                new StringContent(JsonSerializer.Serialize(patchedObject, JsonSerializerOptions), Encoding.UTF8, "application/json-patch+json"));
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpPatchResponseMessage.
            var responseMsg = new HttpPatchResponseMessage<TOut>();
            responseMsg.Status = response.StatusCode;
            responseMsg.Content = await response.Content.ReadAsStringAsync();
            var responsePayload = JObject.Parse(responseMsg.Content);
            if (response.StatusCode == HttpStatusCode.OK)
                {
                // Successful response. Deserialize the object returned. This is the full representation
                // of the instance that was just updated, including the updated values.
                responseMsg.UpdatedInstance = responsePayload.First.First.ToObject<TOut>();
                }
            else
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }
        #endregion

        #region PUT
        internal async Task<HttpResponseMessage> MakeFileUploadCall(string url, System.IO.Stream content)
            {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("x-ms-blob-type", "BlockBlob");
            request.Content = new StreamContent(content);

            using var response = await client.SendAsync(request);
            var responseMsg = new HttpResponseMessage();
            responseMsg.Status = response.StatusCode;
            if (!response.IsSuccessStatusCode)
                {
                // There was an error. Putting response to content while
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                }
            return responseMsg;
            }
        #endregion

        #region DELETE
        internal async Task<HttpResponseMessage> MakeDeleteCall(string url, Dictionary<string, string> customHeaders = null)
            {
            // Add any additional headers if applicable
            AddCustomHeaders(storageClient, customHeaders);

            // Construct full url and then make the POST call
            using var response = await storageClient.DeleteAsync(GetFullUrl(url));
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                // You should implement retry logic for TooManyRequests (429) errors and possibly others like GatewayTimeout or ServiceUnavailable
                }

            // Copy/Deserialize the response into custom HttpResponseMessage.
            var responseMsg = new HttpResponseMessage();
            responseMsg.Status = response.StatusCode;
            if (response.StatusCode != HttpStatusCode.NoContent)
                {
                // There was an error. Deserialize the error details and return.
                responseMsg.Content = await response.Content.ReadAsStringAsync();
                var responsePayload = JObject.Parse(responseMsg.Content);
                responseMsg.ErrorDetails = responsePayload["error"]?.ToObject<ErrorDetails>();
                }
            return responseMsg;
            }
        #endregion

        #region Private Methods

        private void AddCustomHeaders(HttpClient client, Dictionary<string, string> customHeaders = null)
            {
            if (customHeaders != null)
                {
                foreach (var ch in customHeaders)
                    {
                    client.DefaultRequestHeaders.Add(ch.Key, ch.Value);
                    }
                }
            }
        private static JsonSerializerOptions JsonSerializerOptions
            {
            get
                {
                var options = new JsonSerializerOptions
                    {
                    IgnoreNullValues = true,
                    WriteIndented = true,
                    AllowTrailingCommas = false,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    Converters = {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    };
                return options;
                }
            }

        private static string GetFullUrl(string url)
            {
            return url.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ? url : $"{API_BASE_URL}{url}";
            }
        #endregion
        }

    #region Supporting Classes
    internal class HttpResponseMessage
        {
        public HttpStatusCode Status
            {
            get; set;
            }
        public string Content
            {
            get; set;
            }
        public ErrorDetails ErrorDetails
            {
            get; set;
            }
        }

    internal class HttpPostResponseMessage<T> : HttpResponseMessage
        {
        public T Instance
            {
            get; set;
            }
        }
    internal class HttpPatchResponseMessage<T> : HttpResponseMessage
        {
        public T UpdatedInstance
            {
            get; set;
            }
        }
    internal class HttpGetResponseMessage<T> : HttpResponseMessage
        {
        public T Instances
            {
            get; set;
            }
        }
    internal class HttpGetFoldersAndFilesResponseMessage<T> : HttpResponseMessage
        where T : PaginationLinks
        {
        public ItemsDetails<T> Instances
            {
            get; set;
            }
        }
    internal class HttpGetSingleResponseMessage<T> : HttpResponseMessage
        {
        public T Instance
            {
            get; set;
            }
        }
    #endregion
    }
