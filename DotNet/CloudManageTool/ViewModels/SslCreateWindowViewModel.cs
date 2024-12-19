using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Ssl.V20191205.Models;
using TencentCloud.Ssl.V20191205;
using System.Windows;
using CloudManageTool.Models;
using System.Text.RegularExpressions;

namespace CloudManageTool.ViewModels
{
    public class SslCreateWindowViewModel : BindableBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }

        private DvAuthMethodEnum _authMethod;
        public DvAuthMethodEnum AuthMethod
        {
            get { return _authMethod; }
            set { SetProperty(ref _authMethod, value); }
        }

        public DelegateCommand CancelCommand { get; }

        public AsyncDelegateCommand CreateSslCommand { get; }

        private readonly PlatFormSecret _secret;
        public SslCreateWindowViewModel(PlatFormSecret secret)
        {
            CancelCommand = new DelegateCommand(() => { RequestClose.Invoke(ButtonResult.Cancel); });
            CreateSslCommand = new AsyncDelegateCommand(CreateSsl);
            _secret = secret;
            AuthMethod = DvAuthMethodEnum.DNS_AUTO;
        }

        private async Task CreateSsl()
        {
            if (string.IsNullOrWhiteSpace(Domain))
            {
                MessageBox.Show("请先填写需要证书绑定域名");
                return;
            }
            else if (!IsValidDomain(Domain))
            {
                MessageBox.Show("域名格式不正确");
                return;
            }
            else if (Domain.StartsWith("*"))
            {
                MessageBox.Show("泛域名不支持申请免费证书");
                return;
            }
            try
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                SslClient client = new SslClient(cred, "");
                ApplyCertificateRequest req = new ApplyCertificateRequest();
                req.DomainName = Domain;
                req.DvAuthMethod = AuthMethod.ToString();
                if (AuthMethod == DvAuthMethodEnum.DNS_AUTO)
                {
                    req.DeleteDnsAutoRecord = true;
                }
                ApplyCertificateResponse resp = await client.ApplyCertificate(req);
                MessageBox.Show("提交证书申请成功");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "提交证书申请失败");
            }
        }

        bool IsValidDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            string pattern = @"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)\.[A-Za-z]{2,}$";
            return Regex.IsMatch(domain, pattern);
        }
    }
}
