using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Volo.Abp.DependencyInjection;

namespace Abp.MyConsoleApp;

public class SharePointApiService : SharePointBaseService, ITransientDependency, ISharePointService
{
    private readonly ILogger<SharePointApiService> _logger;
    private readonly SharePointSettings _sharePointSettings;

    private readonly string _apiBaseUrl;
    private readonly string _scope;
    private readonly string _loginUrl;

    private string Token { get; set; }

    public SharePointApiService(ILogger<SharePointApiService> logger,
        IOptions<SharePointSettings> sharePointSettings)
    {
        _logger = logger;
        _sharePointSettings = sharePointSettings.Value;
        _scope = "https://graph.microsoft.com/.default";
        _loginUrl = $"https://login.microsoftonline.com/{_sharePointSettings.TenantId}/oauth2/v2.0/token";
        _apiBaseUrl = "https://graph.microsoft.com";
        if (_sharePointSettings.IsChina)
        {
            _apiBaseUrl = "https://microsoftgraph.chinacloudapi.cn";
            _scope = "https://microsoftgraph.chinacloudapi.cn/.default";
            _loginUrl = $"https://login.partner.microsoftonline.cn/{_sharePointSettings.TenantId}/oauth2/v2.0/token";
        }
    }

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetAccessTokenAsync()
    {
        using (var client = new HttpClient())
        {
            var requestParameters = new Dictionary<string, string>
            {
                { "client_id", _sharePointSettings.ClientId },
                { "client_secret", _sharePointSettings.ClientSecret },
                { "grant_type", "client_credentials" },
                { "scope", _scope }
            };
            var response = await client.PostAsync(_loginUrl,
                new FormUrlEncodedContent(requestParameters));
            var jsonResult = await response.Content.ReadAsStringAsync();

            Token = JObject.Parse(jsonResult).Value<string>("access_token");
            return Token;
        }
    }

    private async Task<string> GetApiAsync(string url)
    {
        if (Token.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Token不能为空，请先调用方法：GetAccessToken");
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");
            var response = await client.GetAsync(url);
            var jsonResult = await response.Content.ReadAsStringAsync();

            return jsonResult;
        }
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<User>> GetUsersAsync()
    {
        var url = $"{_apiBaseUrl}/v1.0/users";
        var result = await GetApiAsync(url);
        var jObject = JObject.Parse(result);
        var array = (JArray)jObject["value"];
        var users = array.ToObject<List<User>>();
        return users;
    }

    public async Task<List<Organization>> GetOrgs()
    {
        var url = $"{_apiBaseUrl}/v1.0/organization";
        var result = await GetApiAsync(url);
        var jObject = JObject.Parse(result);
        var array = (JArray)jObject["value"];
        var orgs = array.ToObject<List<Organization>>();
        return orgs;
    }

    /// <summary>
    /// 获取部门列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Group>> GetGroupsAsync()
    {
        //
        var url = $"{_apiBaseUrl}/v1.0/groups";
        var result = await GetApiAsync(url);
        var jObject = JObject.Parse(result);
        var array = (JArray)jObject["value"];
        var orgs = array.ToObject<List<Group>>();
        return orgs;
    }


    /// <summary>
    /// 获取部门详情
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public async Task<Group> GetGroupDetailAsync(string groupId)
    {
        //
        var url = $"{_apiBaseUrl}/v1.0/groups{groupId}";
        var result = await GetApiAsync(url);
        var jObject = JObject.Parse(result);
        var orgs = jObject.ToObject<Group>();
        return orgs;
    }

    public async Task<List<User>> GetGroupMembers(string groupId)
    {
        var url = $"{_apiBaseUrl}/v1.0/groups/{groupId}/members";
        var result = await GetApiAsync(url);
        var jObject = JObject.Parse(result);
        var array = (JArray)jObject["value"];
        var users = array.ToObject<List<User>>();
        return users;
    }


    /// <summary>
    /// 获取用户OneDrive信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<LicenseDetails>> GetUserLicenseDetailAsync(string userId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/users/{userId}/licenseDetails");

        var jObject = JObject.Parse(apiResult);
        if (jObject.Property("value") != null)
        {
            var array = (JArray)jObject["value"];
            var result = array.ToObject<List<LicenseDetails>>();
            return result;
        }

        return new List<LicenseDetails>();
    }

    /// <summary>
    /// 获取用户OneDrive信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Drive> GetUserDriveAsync(string userId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/users/{userId}/drive");
        var result = JsonConvert.DeserializeObject<Drive>(apiResult);
        return result;
    }

    /// <summary>
    /// 获取用户onedrive根目录数据
    /// </summary>
    /// <param name="driveId"></param>
    /// <returns></returns>
    public async Task<DriveItem> GetDriveRootAsync(string driveId)
    {
        //todo 待验证
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/drives/{driveId}/root");
        var result = JsonConvert.DeserializeObject<DriveItem>(apiResult);
        return result;
    }

    /// <summary>
    /// 获取指定目录下属列表
    /// </summary>
    /// <param name="driveId"></param>
    /// <returns></returns>
    public async Task<List<DriveItem>> GetDriveItemsAsync(string driveId, string driveItemId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/drives/{driveId}/items/{driveItemId}/children");

        /*
         * 异常情况 用户OneDrive中无数据
{
    "error": {
        "code": "BadRequest",
        "message": "Resource not found for the segment 'children'.",
        "innerError": {
            "date": "2023-07-31T01:45:15",
            "request-id": "1e75b911-e20d-458b-bbb8-5b05bafdec58",
            "client-request-id": "1e75b911-e20d-458b-bbb8-5b05bafdec58"
        }
    }
}
         */
        var jObject = JObject.Parse(apiResult);
        if (jObject.Property("value") != null)
        {
            var array = (JArray)jObject["value"];
            var result = array.ToObject<List<DriveItem>>();
            return result;
        }

        return new List<DriveItem>();
    }

    /// <summary>
    /// 获取文件权限
    /// </summary>
    /// <param name="driveId"></param>
    /// <param name="driveItemId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<Permission>> GetDriveItemPermissionsAsync(string driveId, string driveItemId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/drives/{driveId}/items/{driveItemId}/permissions");
        var jObject = JObject.Parse(apiResult);
        if (jObject.Property("value") != null)
        {
            var array = (JArray)jObject["value"];
            var result = array.ToObject<List<Permission>>();
            return result;
        }

        return new List<Permission>();
    }

    public async Task DownloadFileWithApiAsync(SharePointFileInfo fileInfo)
    {
        try
        {
            _logger.LogInformation(
                $"开始下载文件：{fileInfo.FileInfo.Name}到{fileInfo.FileFullPath}，使用线程 {Thread.CurrentThread.ManagedThreadId}");
            // var httpClientHandler = new HttpClientHandler
            // {
            //     SslProtocols = SslProtocols.Tls
            // };
            using (var client = new HttpClient())
            {
                var url = $"{_apiBaseUrl}/v1.0/drives/{fileInfo.DriveId}/items/{fileInfo.DriveItemId}/content";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");

                var response = await client.GetAsync(url);
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream =
                           new FileStream(Path.Combine(AppContext.BaseDirectory, fileInfo.FileFullPath),
                               FileMode.Create,
                               FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
            }

            _logger.LogInformation(
                $"文件：{fileInfo.FileInfo.Name} 已成功下载到{fileInfo.FileFullPath}，使用线程 {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"文件下载失败：{fileInfo.FileInfo.Name},目标路径：{fileInfo.FileFullPath}");
        }
    }

    public async Task<List<Site>> GetSitesAsync()
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/sites");

        var jObject = JObject.Parse(apiResult);
        if (jObject.Property("value") != null)
        {
            var array = (JArray)jObject["value"];
            var result = array.ToObject<List<Site>>();
            return result;
        }

        return new List<Site>();
    }

    public async Task<Site> GetSiteDetailAsync(string siteId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/sites/{siteId}");
        var result = JsonConvert.DeserializeObject<Site>(apiResult);
        return result;
    }

    public async Task<Drive> GetSiteDriveAsync(string siteId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/sites/{siteId}/drive");
        var result = JsonConvert.DeserializeObject<Drive>(apiResult);
        return result;
    }

    public async Task<List<Site>> GetGroupSitesAsync(string groupId)
    {
        var apiResult = await GetApiAsync($"{_apiBaseUrl}/v1.0/groups/{groupId}/sites");

        var jObject = JObject.Parse(apiResult);
        if (jObject.Property("value") != null)
        {
            var array = (JArray)jObject["value"];
            var result = array.ToObject<List<Site>>();
            return result;
        }

        return new List<Site>();
    }
}