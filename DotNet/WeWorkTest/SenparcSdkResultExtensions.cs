using Senparc.Weixin.Entities;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work.AdvancedAPIs.OAuth2;

namespace WeWorkTest
{

    public class WeWorkUserDetailResult : GetUserDetailResult
    {
        public string biz_mail { get; set; }
    }

    public class WeWorkDepartmentMemberInfoResult : WorkJsonResult
    {
        public List<WeWorkGetMemberResult> userlist { get; set; }
    }

    public class WeWorkGetMemberResult: GetMemberResult
    {
        /// <summary>
        /// 直属上级
        /// </summary>
        public string[] direct_leader { get; set; }
    }
}
