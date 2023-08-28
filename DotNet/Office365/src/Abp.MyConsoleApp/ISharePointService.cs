using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph.Models;

namespace Abp.MyConsoleApp;

public interface ISharePointService
{
    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <returns></returns>
    Task<string> GetAccessTokenAsync();

    #region 组组架构

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <returns></returns>
    Task<List<User>> GetUsersAsync();

    /// <summary>
    /// 获取部门列表
    /// </summary>
    /// <returns></returns>
    Task<List<Group>> GetGroupsAsync();

    /// <summary>
    /// 获取部门详情
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    Task<Group> GetGroupDetailAsync(string groupId);

    /// <summary>
    /// 获取组下属成员
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    Task<List<User>> GetGroupMembers(string groupId);

    #endregion

    /// <summary>
    /// 获取用户授权License
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<LicenseDetails>> GetUserLicenseDetailAsync(string userId);

    #region OneDrive

    /// <summary>
    /// 获取用户OneDrive信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Drive> GetUserDriveAsync(string userId);

    /// <summary>
    /// 获取OneDrive根目录信息
    /// </summary>
    /// <param name="driveId">OneDrive ID</param>
    /// <returns></returns>
    Task<DriveItem> GetDriveRootAsync(string driveId);

    /// <summary>
    /// 获取OneDrive指定目录下属文件/文件夹列表
    /// </summary>
    /// <param name="driveId">OneDrive ID</param>
    /// <param name="driveItemId">文件夹ID</param>
    /// <returns></returns>
    Task<List<DriveItem>> GetDriveItemsAsync(string driveId, string driveItemId);

    /// <summary>
    /// 获取OneDrive指定文件权限
    /// </summary>
    /// <param name="driveId">OneDrive ID</param>
    /// <param name="driveItemId">文件ID</param>
    /// <returns></returns>
    Task<List<Permission>> GetDriveItemPermissionsAsync(string driveId, string driveItemId);

    #endregion

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    Task DownloadFileWithApiAsync(SharePointFileInfo fileInfo);

    #region SharePoint Sites

    /// <summary>
    /// 获取所有站点
    /// </summary>
    /// <returns></returns>
    Task<List<Site>> GetSitesAsync();

    /// <summary>
    /// 获取站点详情
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    Task<Site> GetSiteDetailAsync(string siteId);

    /// <summary>
    /// 获取SharePoint站点对应的OneDrive
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    Task<Drive> GetSiteDriveAsync(string siteId);

    /// <summary>
    /// 获取部门站点
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    Task<List<Site>> GetGroupSitesAsync(string groupId);

    #endregion
}