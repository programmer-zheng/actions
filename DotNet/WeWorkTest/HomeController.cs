using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Extensions;
using Senparc.Weixin.CommonAPIs;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.MailList;
using Senparc.Weixin.Work.AdvancedAPIs.OAuth2;
using Senparc.Weixin.Work.Containers;
using System.Net;

namespace WeWorkTest
{
    public class HomeController : Controller
    {
        private readonly string CorpId;
        private readonly string CorpSecret;
        private readonly string AgentId;

        private readonly IConfiguration _configuration;

        private readonly string AppKey;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            CorpId = _configuration.GetValue<string>("CorpId");
            CorpSecret = _configuration.GetValue<string>("AppSecret");
            AgentId = _configuration.GetValue<string>("AgentId");
            AppKey = AccessTokenContainer.BuildingKey(CorpId, CorpSecret);
        }

        public IActionResult Index()
        {
            return Content("Hello World");
        }

        private string GetAccessToken()
        {
            return AccessTokenContainer.GetToken(AppKey);
        }

        [Route("/test")]
        public IActionResult Test(IConfiguration configuration)
        {


            var accessToken = GetAccessToken();


            var departmentList = MailListApi.GetDepartmentList(accessToken);

            var rootUsers = MailListApi.GetDepartmentMemberInfo(accessToken, departmentList.department.First().id, 0);

            var mapperRules = new Dictionary<string, string>();
            mapperRules["UserName"] = "userid";
            mapperRules["OrganizationRole"] = "is_leader_in_dept";
            mapperRules["Status"] = "status";
            mapperRules["ChineseName"] = "name";

            // 通过反射将Senparc SDK中的企业微信用户转换为本地 User 对象
            string json = string.Empty;
            foreach (var item in rootUsers.userlist)
            {
                var user = item.ToMapperModel<GetMemberResult, User>(mapperRules);
                json = user.ToJson();
            }
            // var list = new List<GetMemberResult>();
            // foreach (var department in departmentList?.department)
            // {
            //     var memberInfoResult = MailListApi.GetDepartmentMemberInfo(accessToken, department.id, 0);
            //     var users = memberInfoResult.userlist;
            //     list.AddRange(users);
            // }
            return Content(json);
        }

        [Route("/WeWork")]
        public IActionResult WeWork()
        {
            var url = WebUtility.UrlEncode($"{Request.Scheme}://{Request.Host}/WeWorkCallback");
            var weWorkRedirectUrl = OAuth2Api.GetCode(CorpId, url, "", AgentId, scope: "snsapi_privateinfo");
            return Redirect(weWorkRedirectUrl);
        }

        [Route("/WeWorkCallback")]
        public async Task<IActionResult> WeWorkCallback(string code, string state)
        {
            var accessToken = GetAccessToken();
            var getUserInfoResult = OAuth2Api.GetUserId(accessToken, code);
            var userId = getUserInfoResult.UserId;
            var ticket = getUserInfoResult.user_ticket;
            // Senparc.Weixin.Work 中未添加企业邮箱，使用扩展类接收企业微信API返回
            //var userDetail = await OAuth2Api.GetUserDetailAsync(accessToken, ticket);
            var userDetail = await GetWeWorkUserDetail(accessToken, ticket);
            return Json(userDetail);
        }

        public async Task<WeWorkUserDetailResult> GetWeWorkUserDetail(string accessToken, string user_ticket)
        {
            string urlFormat = Senparc.Weixin.Config.ApiWorkHost + "/cgi-bin/user/getuserdetail?access_token={0}";
            var data = new
            {
                user_ticket = user_ticket
            };
            return await CommonJsonSend.SendAsync<WeWorkUserDetailResult>(accessToken, urlFormat, data).ConfigureAwait(continueOnCapturedContext: false);
        }
    }


}
