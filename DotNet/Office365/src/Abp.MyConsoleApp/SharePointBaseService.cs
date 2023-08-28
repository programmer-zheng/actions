using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Graph.Models;

namespace Abp.MyConsoleApp;

public abstract class SharePointBaseService
{
    /// <summary>
    /// 判断用户是否有OneDrive授权
    /// </summary>
    /// <param name="licenseList"></param>
    /// <returns></returns>
    public bool CheckUserHasOneDriveLicense(List<LicenseDetails> licenseList)
    {
        var containsOneDrive = licenseList.Any(license =>
            license.ServicePlans != null &&
            license.ServicePlans.Any(t =>
                t.ProvisioningStatus.Equals("success", StringComparison.OrdinalIgnoreCase) &&
                t.ServicePlanName.Equals("SHAREPOINTENTERPRISE", StringComparison.OrdinalIgnoreCase)));

        return containsOneDrive;
    }
}