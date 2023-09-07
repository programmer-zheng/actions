using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeWorkTest
{
    public class SyncUserInfoModel
    {
        public User User { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }

        public string ChineseName { get; set; }

        public int Status { get; set; }

        public int? Gender { get; set; }

        public OrganizationRoleEnum OrganizationRole { get; set; }

        public string OriginalID { get; set; }


    }

    public enum OrganizationRoleEnum
    {
        /// <summary>
        /// 员工
        /// </summary>
        staff = 0,

        /// <summary>
        /// 主管
        /// </summary>
        manager = 1
    }
}
