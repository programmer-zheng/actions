using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace Abp.MyConsoleApp;

public class SharePointGraphService : SharePointBaseService, ITransientDependency, ISharePointService
{
    private const string TempFileFolderName = "temp";
    
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ClientSecretCredential _clientSecretCredential;
    private readonly string[] _scopes;
    private readonly ILogger<SharePointGraphService> _logger;
    private readonly SharePointSettings _sharePointSettings;

    public string Token { get; private set; }

    public SharePointGraphService(IOptions<SharePointSettings> settings, ILogger<SharePointGraphService> logger)
    {
        _logger = logger;
        _sharePointSettings = settings.Value;
        var authorityHost = AzureAuthorityHosts.AzurePublicCloud;
        _scopes = new[] { "https://graph.microsoft.com/.default" };
        string baseUrl = string.Empty;
        if (_sharePointSettings.IsChina)
        {
            authorityHost = AzureAuthorityHosts.AzureChina;
            _scopes = new[] { "https://microsoftgraph.chinacloudapi.cn/.default" };

            // 不设置的话，默认是全球版的地址（https://graph.microsoft.com/v1.0）
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/src/Microsoft.Graph/Generated/BaseGraphServiceClient.cs#L381
            baseUrl = "https://microsoftgraph.chinacloudapi.cn/v1.0";
        }

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = authorityHost,
        };

        // 访问令牌
        _clientSecretCredential = new ClientSecretCredential(
            _sharePointSettings.TenantId, _sharePointSettings.ClientId, _sharePointSettings.ClientSecret, options);

        // 创建 GraphServiceClient 实例
        _graphServiceClient = new GraphServiceClient(_clientSecretCredential, _scopes, baseUrl);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var context = new TokenRequestContext(_scopes);
        try
        {
            var token = await _clientSecretCredential.GetTokenAsync(context);
            Token = token.Token;
            return Token;
        }
        catch (Exception e)
        {
            _logger.LogError("获取AccessToken失败", e);
            return string.Empty;
        }
    }

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var userResponse = await _graphServiceClient.Users.GetAsync();
            return userResponse?.Value;
        }
        catch (ODataError e)
        {
            return null;
        }
        catch (ServiceException ex)
        {
            _logger.LogError("Error:");
            _logger.LogError($"Message: {ex.Message}");
            _logger.LogError($"InnerException: {ex.InnerException}");
            if (ex.RawResponseBody != null)
            {
                _logger.LogError(ex.RawResponseBody);
                // _logger.LogError($"Error.Code: {ex.Error.Code}");
                // _logger.LogError($"Error.Message: {ex.Error.Message}");
            }

            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("获取用户列表失败", e);
            throw;
        }
    }

    public async Task<List<Group>> GetGroupsAsync()
    {
        var groupResponse = await _graphServiceClient.Groups.GetAsync();

        if (groupResponse != null) return groupResponse.Value;
        return new List<Group>();
    }

    public async Task<Group> GetGroupDetailAsync(string groupId)
    {
        var group = await _graphServiceClient.Groups[groupId].GetAsync();
        return group;
    }

    public async Task<List<User>> GetGroupMembers(string groupId)
    {
        var response = await _graphServiceClient.Groups[groupId].Members.GetAsync();
        if (response.Value != null)
        {
            // 返回的DirectoryObject，无法直接访问除人员ID外的其他属性，但返回的数据却是User类型
            var str = JsonConvert.SerializeObject(response.Value);
            return JsonConvert.DeserializeObject<List<User>>(str);
        }

        return new List<User>();
    }

    public async Task<List<LicenseDetails>> GetUserLicenseDetailAsync(string userId)
    {
        var response = await _graphServiceClient.Users[userId].LicenseDetails.GetAsync();
        if (response.Value != null)
        {
            return response.Value;
        }

        return new List<LicenseDetails>();
    }

    public async Task<Drive> GetUserDriveAsync(string userId)
    {
        try
        {
            var drive = await _graphServiceClient.Users[userId].Drive.GetAsync();
            return drive;
        }
        catch (ODataError e)
        {
            return null;
        }
        catch (ServiceException ex)
        {
            _logger.LogError("Error:");
            _logger.LogError($"Message: {ex.Message}");
            _logger.LogError($"InnerException: {ex.InnerException}");
            if (ex.RawResponseBody != null)
            {
                _logger.LogError(ex.RawResponseBody);
                // _logger.LogError($"Error.Code: {ex.Error.Code}");
                // _logger.LogError($"Error.Message: {ex.Error.Message}");
            }

            throw;
        }
    }

    public async Task<DriveItem> GetDriveRootAsync(string driveId)
    {
        var root = await _graphServiceClient.Drives[driveId].Root.GetAsync();
        return root;
    }

    public async Task<List<DriveItem>> GetDriveItemsAsync(string driveId, string driveItemId)
    {
        var response = await _graphServiceClient.Drives[driveId].Items[driveItemId].Children.GetAsync();
        if (response.Value != null)
        {
            return response.Value;
        }

        return new List<DriveItem>();
    }

    public async Task<List<Permission>> GetDriveItemPermissionsAsync(string driveId, string driveItemId)
    {
        var response = await _graphServiceClient.Drives[driveId].Items[driveItemId].Permissions.GetAsync();
        if (response.Value != null)
        {
            return response.Value;
        }

        return new List<Permission>();
    }


    public async Task Download(SharePointFileInfo fileInfo, string fileName, long contentLength, string token)
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                if (contentLength > 0 && contentLength > 1024 * 1024 * 1)
                {
                    var url =
                        $"https://graph.microsoft.com/v1.0/drives/{fileInfo.DriveId}/items/{fileInfo.DriveItemId}/content";
                    int numberOfThreads = (int)Math.Floor((double)contentLength / (1024 * 1024 * 1));

                    if (numberOfThreads > 10)
                    {
                        numberOfThreads = 10;
                    }

                    var blockSize = (long)Math.Ceiling((double)contentLength / numberOfThreads);

                    var tasks = new Task[numberOfThreads];
                    var chunkFilePaths = new ConcurrentQueue<string>(); // 分片文件路径数组
                    for (int i = 0; i < numberOfThreads; i++)
                    {
                        long startByte = i * blockSize;
                        long endByte = (i == numberOfThreads - 1) ? contentLength - 1 : startByte + blockSize - 1;
                        var tempName = $"{fileName}_block_{i}.tmp";
                        chunkFilePaths.Enqueue(tempName);
                        tasks[i] = DownloadBlockAsync(client, url, startByte, endByte, tempName);
                    }

                    Task.WaitAll(tasks);
                    await MergeFilesAsync(chunkFilePaths, fileInfo.FileFullPath);
                }
                else
                {
                    await DownloadFileWithApiAsync(fileInfo);
                }
            }

            _logger.LogDebug(
                $"文件：{fileName} 已成功下载，使用线程 {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"文件{fileName} 下载失败 文件大小：{contentLength}");
        }
    }

    async Task DownloadBlockAsync(HttpClient httpClient, string fileUrl, long startByte, long endByte, string fileName)
    {
        try
        {
            _logger.LogDebug(
                $"开始下载分片：{fileName}，使用线程 {Thread.CurrentThread.ManagedThreadId} startByte:{startByte},endByte:{endByte}");
            var range = $"bytes={startByte}-{endByte}";
            var request = new HttpRequestMessage(HttpMethod.Get, fileUrl);
            request.Headers.Add("Range", range);

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(Path.Combine(AppContext.BaseDirectory, TempFileFolderName, fileName)))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"分片{fileName}下载失败");
        }
    }

    async Task MergeFilesAsync(ConcurrentQueue<string> sourceFilePaths, string outputPath)
    {
        using (var output = File.Create(Path.Combine(AppContext.BaseDirectory, outputPath)))
        {
            foreach (var sourceFilePath in sourceFilePaths)
            {
                using (var sourceStream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, TempFileFolderName, sourceFilePath)))
                {
                    await sourceStream.CopyToAsync(output);
                }
            }
        }

        // 删除分片文件
        foreach (var sourceFilePath in sourceFilePaths)
        {
            File.Delete(Path.Combine(AppContext.BaseDirectory, TempFileFolderName, sourceFilePath));
        }
    }


    public async Task DownloadFileWithApiAsync(SharePointFileInfo fileInfo)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, fileInfo.FileFullPath);
            _logger.LogDebug(
                $"开始下载文件：{fileInfo.FileInfo.Name}到{path}，使用线程 {Thread.CurrentThread.ManagedThreadId}");

            var response = _graphServiceClient.Drives[fileInfo.DriveId].Items[fileInfo.DriveItemId].Content
                .GetAsync();
            using (var contentStream = await response)
            {
                using (FileStream fileStream =
                       new FileStream(path,
                           FileMode.Create,
                           FileAccess.Write, FileShare.None))
                {
                    await contentStream.CopyToAsync(fileStream);
                }
            }

            _logger.LogDebug(
                $"文件：{fileInfo.FileInfo.Name} 已成功下载到{path}，使用线程 {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"文件下载失败：{fileInfo.FileInfo.Name},目标路径：{fileInfo.FileFullPath}");
        }
    }

    public async Task<List<Site>> GetSitesAsync()
    {
        var response = await _graphServiceClient.Sites.GetAsync();
        if (response.Value != null)
        {
            return response.Value;
        }

        return new List<Site>();
    }


    public async Task<Site> GetSiteDetailAsync(string siteId)
    {
        var site = await _graphServiceClient.Sites[siteId].GetAsync();
        return site;
    }

    public async Task<Drive> GetSiteDriveAsync(string siteId)
    {
        try
        {
            var drive = await _graphServiceClient.Sites[siteId].Drive.GetAsync();
            return drive;
        }
        catch (ODataError e)
        {
            return null;
        }
        catch (ServiceException ex)
        {
            _logger.LogError("Error:");
            _logger.LogError($"Message: {ex.Message}");
            _logger.LogError($"InnerException: {ex.InnerException}");
            if (ex.RawResponseBody != null)
            {
                _logger.LogError(ex.RawResponseBody);
                // _logger.LogError($"Error.Code: {ex.Error.Code}");
                // _logger.LogError($"Error.Message: {ex.Error.Message}");
            }

            throw;
        }
    }

    public async Task<List<Site>> GetGroupSitesAsync(string groupId)
    {
        var response = await _graphServiceClient.Groups[groupId].Sites.GetAsync();
        if (response.Value != null)
        {
            return response.Value;
        }

        return new List<Site>();
    }
}