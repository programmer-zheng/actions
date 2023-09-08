using Senparc.Weixin.CommonAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work;
using Senparc.CO2NET.Extensions;

namespace WeWorkTest
{
    public static class SenparcSdkApiExtensions
    {
        public static async Task<WeWorkUserDetailResult> GetWeWorkUserDetail(string accessToken, string user_ticket)
        {
            string urlFormat = Senparc.Weixin.Config.ApiWorkHost + "/cgi-bin/user/getuserdetail?access_token={0}";
            var data = new
            {
                user_ticket = user_ticket
            };
            return await CommonJsonSend.SendAsync<WeWorkUserDetailResult>(accessToken, urlFormat, data).ConfigureAwait(continueOnCapturedContext: false);
        }

        public static WeWorkDepartmentMemberInfoResult GetWeWorkDepartmentMemberInfo(string accessTokenOrAppKey, long departmentId, int? fetchChild)
        {
            return ApiHandlerWapper.TryCommonApi(delegate (string accessToken)
            {
                string urlFormat = string.Format(Senparc.Weixin.Config.ApiWorkHost + "/cgi-bin/user/list?access_token={0}&department_id={1}&fetch_child={2}", accessToken.AsUrlData(), departmentId, fetchChild);
                return CommonJsonSend.Send<WeWorkDepartmentMemberInfoResult>(null, urlFormat, null, Senparc.Weixin.CommonJsonSendType.GET);
            }, accessTokenOrAppKey);
        }
    }

}
