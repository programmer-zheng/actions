using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainManageTool
{
    public class PlatFormSecret
    {
        public PlatFormSecret(string id, string key)
        {
            SecretId = id;
            SecretKey = key;
        }
        public string SecretId { get; set; }

        public string SecretKey { get; set; }
    }
}
