using CloudManageTool.Models;
using CloudManageTool.Views;
using Mapster;
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
        public AsyncDelegateCommand OpenCreateNewFreeCertDialogCommand { get; }
        public AsyncDelegateCommand OpenSecurityRdpDialogCommand { get; }
        public AsyncDelegateCommand RefreshCommand { get; }

        public AsyncDelegateCommand<string> RevokeCertificateCommand { get; }
        public AsyncDelegateCommand<string> DeleteCertificateCommand { get; }

        public AsyncDelegateCommand<string> OpenDownloadDialogCommand { get; private set; }



        private readonly PlatFormSecret _secret;
        private readonly IDialogService _dialogService;

        public SslManageWindowViewModel(PlatFormSecret platFormSecret, IDialogService dialogService)
        {
            _secret = platFormSecret;

            OpenDownloadDialogCommand = new AsyncDelegateCommand<string>(OpenDownloadDialog);
            OpenCreateNewFreeCertDialogCommand = new AsyncDelegateCommand(OpenCreateNewFreeCertDialog);
            OpenSecurityRdpDialogCommand = new AsyncDelegateCommand(OpenSecurityRdpDialog);
            RevokeCertificateCommand = new AsyncDelegateCommand<string>(RevokeCertificate);
            DeleteCertificateCommand = new AsyncDelegateCommand<string>(DeleteCertificate);

            RefreshCommand = new AsyncDelegateCommand(LoadSslList);
            Task.Run(async () =>
            {
                await LoadSslList();
            });
            _dialogService = dialogService;
        }

        private async Task DeleteCertificate(string certificateId)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                SslClient client = new SslClient(cred, "");
                DeleteCertificateRequest req = new DeleteCertificateRequest();
                req.CertificateId = certificateId;
                DeleteCertificateResponse resp = client.DeleteCertificateSync(req);

                await LoadSslList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "删除证书出现异常");
            }
        }

        // 吊销证书
        private async Task RevokeCertificate(string certificateId)
        {
            var result = MessageBox.Show("确认吊销吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Credential cred = new Credential
                    {
                        SecretId = _secret.SecretId,
                        SecretKey = _secret.SecretKey
                    };
                    SslClient client = new SslClient(cred, "");
                    RevokeCertificateRequest req = new RevokeCertificateRequest();
                    req.CertificateId = certificateId;
                    RevokeCertificateResponse resp = client.RevokeCertificateSync(req);

                    await LoadSslList();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "吊销证书出现异常");
                }
            }
        }

        private async Task OpenSecurityRdpDialog()
        {
            await _dialogService.ShowDialogAsync(nameof(SslSecurityRdpWindow));

        }

        private async Task OpenCreateNewFreeCertDialog()
        {
            var dialogResult = await _dialogService.ShowDialogAsync(nameof(SslCreateWindow));
            if (dialogResult.Result == ButtonResult.OK)
            {
                await LoadSslList();
            }
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

        private List<SslListDto> _sslList;
        public List<SslListDto> SslCertList
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
                SslCertList = resp.Certificates.ToList().Adapt<List<SslListDto>>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "加载证书列表出错");
            }
        }
    }
}
