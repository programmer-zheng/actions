using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Volo.Abp;

namespace Abp.MyConsoleApp;

public class MyConsoleAppHostedService : IHostedService
{
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;

    private readonly SharePointGraphService _sharePointService;
    private readonly SharePointApiService _sharePointApiService;
    private readonly ILogger<MyConsoleAppHostedService> _logger;

    private readonly ConcurrentDictionary<string, SharePointFileInfo> _dictionary;

    private readonly IConfiguration _configuration;

    private readonly string _tenantId;
    private readonly long targetSize;
    private long _totalSize;

    public MyConsoleAppHostedService(IAbpApplicationWithExternalServiceProvider abpApplication,
        ILogger<MyConsoleAppHostedService> logger, SharePointGraphService sharePointService,
        IConfiguration configuration, SharePointApiService apiService)
    {
        _sharePointApiService = apiService;
        _sharePointService = sharePointService;
        _abpApplication = abpApplication;
        _logger = logger;
        _configuration = configuration;
        _dictionary = new ConcurrentDictionary<string, SharePointFileInfo>();
        _tenantId = _configuration.GetValue<string>("SharePointSetting:TenantId");
        /*
         * 1 KB = 1024 bytes
         * 1 MB = 1024 KB = (1024 * 1024) bytes = 1048576
         * 1 GB = 1024 MB = 1024 * 1024 KB = (1024 * 1024 * 1024) bytes = 1073741824
         * 1 TB = 1024 GB = 1024 * 1024 MB = 1024 * 1024 * 1024 KB = (1024 * 1024 * 1024 * 1024) bytes = 1099511627776
         */

        targetSize =
            _configuration.GetValue<long>("TargetDownloadFileSize"); //unchecked(1024 * 1024 * 1024 * 1024);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // // 部门
        var groups = await _sharePointService.GetGroupsAsync();
        _logger.LogInformation($"共有以下{groups.Count}个部门：\n{string.Join("\n", groups.Select(t => t.DisplayName))}");
        foreach (var group in groups)
        {
            _logger.LogInformation($"============={group.DisplayName} 成员有：=====================");
            // 部门下属成员情况
            var groupUsers = await _sharePointService.GetGroupMembers(group.Id);
            _logger.LogInformation(string.Join("、", groupUsers.Select(t => t.DisplayName)));
        }

        //  // 用户
        var users = await _sharePointService.GetUsersAsync();
        _logger.LogInformation($"用户数量 ：{users.Count}");
        _logger.LogInformation(string.Join("\n", users.Select(t => $"{t.Mail}（{t.DisplayName}）")));
        foreach (var user in users)
        {
            if (_totalSize > targetSize)
            {
                break;
            }

            var userLicense = await _sharePointService.GetUserLicenseDetailAsync(user.Id);
            if (_sharePointService.CheckUserHasOneDriveLicense(userLicense))
            {
                var userDrive = await _sharePointService.GetUserDriveAsync(user.Id);
                if (userDrive != null && !userDrive.Id.IsNullOrWhiteSpace())
                {
                    // _logger.LogInformation($"开始处理用户 {user.DisplayName} {user.Id}数据");
                    await GetDriveRootAndOperateAsync(userDrive, "Users", user.Id);
                }
            }
        }

        var sites = await _sharePointService.GetSitesAsync();
        _logger.LogInformation($"sites count ：{sites.Count}");
        foreach (var site in sites)
        {
            if (_totalSize > targetSize)
            {
                break;
            }

            var siteDrive = await _sharePointService.GetSiteDriveAsync(site.Id);

            if (siteDrive != null && !siteDrive.Id.IsNullOrWhiteSpace())
            {
                _logger.LogDebug($"开始处理站点 {site.DisplayName} {site.Id}数据");
                await GetDriveRootAndOperateAsync(siteDrive, "Sites", site.Id);
            }
        }

        int threadCount = _configuration.GetValue<int>("ThreadCount");
        if (_dictionary?.Count > 0)
        {
            var token = await _sharePointService.GetAccessTokenAsync();
            _logger.LogInformation($"==================== beging download files，共{_dictionary.Count} ====================");
            var start = Stopwatch.GetTimestamp();
            var tasks = new List<Task>();
            foreach (var key in _dictionary.Keys)
            {
                var fileInfo = _dictionary[key];
                Task downloadTask = _sharePointService.Download(fileInfo, fileInfo.FileInfo.Name, fileInfo.FileInfo.Size.Value, token);
                // 当达到最大并行下载数量时，等待任一下载任务完成，然后再添加新的下载任务
                if (tasks.Count >= threadCount)
                {
                    Task.WaitAny(tasks.ToArray());
                    tasks.RemoveAll(t => t.IsCompleted);
                }

                tasks.Add(downloadTask);
            }

            // 等待所有下载任务完成
            Task.WaitAll(tasks.ToArray());
            var stop = Stopwatch.GetTimestamp();
            var seconds = Stopwatch.GetElapsedTime(start, stop).Seconds;
            _logger.LogInformation(
                $"==================== download is complete,total bytes:{_totalSize},Take Time：{seconds} seconds ====================");
        }
    }

    private async Task GetDriveRootAndOperateAsync(Drive drive, string directoryPerfix1, string directoryPerfix2)
    {
        var driveRoot = await _sharePointService.GetDriveRootAsync(drive.Id);
        if (driveRoot != null && !driveRoot.Id.IsNullOrWhiteSpace())
        {
            var directoryPath = Path.Combine("Office365", _tenantId, directoryPerfix1, directoryPerfix2);
            CheckAndCreateDirectory(directoryPath);
            await GetFiles(drive.Id, driveRoot.Id, directoryPath);
        }
    }


    private void CheckAndCreateDirectory(string directoryPath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, directoryPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    private async Task GetFiles(string driveId, string driveItemId, string directoryPath)
    {
        var children = await _sharePointService.GetDriveItemsAsync(driveId, driveItemId);
        foreach (var item in children)
        {
            var permissions = await _sharePointService.GetDriveItemPermissionsAsync(driveId, driveItemId);
            if (permissions != null && permissions.Count > 0)
            {
                foreach (var permission in permissions.Where(t => t.GrantedToIdentitiesV2 != null))
                {
                    // todo 需要排除文件/文件夹拥有者
                    var granted = permission.GrantedToIdentitiesV2;
                    var userNameList = granted.Where(t => t.User != null).Select(t => t.User.DisplayName);
                    var groupNameList = granted.Where(t => t.Group != null).Select(t => t.Group.DisplayName);
                    var userName = string.Join("、", userNameList);
                    var groupName = string.Join("、", groupNameList);
                    var roleName = string.Join("、", permission.Roles);
                    if (!userName.IsNullOrWhiteSpace() || !groupName.IsNullOrWhiteSpace())
                    {
                        var tagName = item.Folder == null ? "文件" : "文件夹";
                        // _logger.LogDebug($"{tagName}：{item.Name} 为 {userName},{groupName} 设置了{roleName}角色");
                    }
                }
            }

            if (item.Folder == null)
            {
                if (_totalSize > targetSize)
                {
                    break;
                }

                if (item.Size.HasValue)
                {
                    _totalSize += item.Size.Value;
                }

                var filePath = Path.Combine(directoryPath, item.Name);
                CheckAndCreateDirectory(directoryPath);
                var flag = _dictionary.TryAdd(item.Id,
                    new SharePointFileInfo()
                    {
                        FileInfo = item,
                        FileFullPath = filePath,
                        DriveItemId = item.Id,
                        DriveId = driveId
                    });
                // if (flag)
                // {
                //     _logger.LogDebug($"文件：{item.Name} {item.Id} 大小：{item.Size} 保存目标位置：{directoryPath}");
                // }
                // else
                // {
                //     _logger.LogDebug(
                //         $"文件：{item.Name} {item.Id},Drive：{driveId} 已存在于{_dictionary[item.Id].FileFullPath}");
                // }
            }
            else
            {
                var path = Path.Combine(directoryPath, item.Name);
                CheckAndCreateDirectory(path);

                await GetFiles(driveId, item.Id, path);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _abpApplication.ShutdownAsync();
    }
}