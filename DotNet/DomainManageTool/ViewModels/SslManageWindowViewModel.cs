using CloudManageTool.Views;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TencentCloud.Common;
using TencentCloud.Ssl.V20191205;
using TencentCloud.Ssl.V20191205.Models;

namespace CloudManageTool.ViewModels
{
    public class SslManageWindowViewModel : BindableBase
    {
        public AsyncDelegateCommand CreateNewFreeCert { get; }
        public AsyncDelegateCommand SearchCommand { get; }

        public AsyncDelegateCommand<string> OpenDownloadDialogCommand { get; private set; }


        private readonly PlatFormSecret _secret;
        private readonly IDialogService _dialogService;

        public SslManageWindowViewModel(PlatFormSecret platFormSecret, IDialogService dialogService)
        {
            _secret = platFormSecret;

            OpenDownloadDialogCommand = new AsyncDelegateCommand<string>(OpenDownloadDialog);

            Task.Run(async () =>
            {
                await LoadSslList();
            });
            _dialogService = dialogService;
        }

        private async Task OpenDownloadDialog(string certificateId)
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("CertificateId", certificateId);
            await _dialogService.ShowDialogAsync(nameof(SslDownloadWindow), dialogParameters);
        }

        private string _keywords;
        public string Keywords
        {
            get { return _keywords; }
            set { SetProperty(ref _keywords, value); }
        }

        private List<Certificates> _sslList;
        public List<Certificates> SslCertList
        {
            get { return _sslList; }
            set { SetProperty(ref _sslList, value); }
        }

        private async Task LoadSslList()
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                SslClient client = new SslClient(cred, "");
                DescribeCertificatesRequest req = new DescribeCertificatesRequest();
                DescribeCertificatesResponse resp = await client.DescribeCertificates(req);
                SslCertList = resp.Certificates.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "加载证书列表出错");
            }
        }
    }
}
