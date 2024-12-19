using CloudManageTool.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using TencentCloud.Common;
using TencentCloud.Ssl.V20191205;
using TencentCloud.Ssl.V20191205.Models;

namespace CloudManageTool.ViewModels
{
    public class SslDownloadWindowViewModel : BindableBase, IDialogAware
    {
        private readonly PlatFormSecret _secret;
        private readonly HttpClient _httpClient;

        private List<SslDownloadDto> _list;
        public List<SslDownloadDto> DownloadList
        {
            get { return _list; }
            set { SetProperty(ref _list, value); }
        }

        public string CertificateId { get; set; }

        public DelegateCommand CancelCommand { get; }

        public DialogCloseListener RequestClose { get; }

        public AsyncDelegateCommand<string> DownloadCertCommand { get; }


        public SslDownloadWindowViewModel(PlatFormSecret secret)
        {
            CancelCommand = new DelegateCommand(() =>
            {
                RequestClose.Invoke(ButtonResult.Cancel);
            });

            DownloadCertCommand = new AsyncDelegateCommand<string>(DownloadCert);

            // nginx tomcat apache iis jks other root
            DownloadList = new List<SslDownloadDto> {
                new (){ Description="Tomcat（pfx格式）",ServiceType="tomcat"},
                new (){ Description="Tomcat（JKS格式）",ServiceType="jks"},
                new (){ Description="Apache（crt文件、key文件）",ServiceType="apache"},
                new (){ Description="Nginx（适用大部分场景）（pem文件、crt文件、key文件）",ServiceType="nginx"},
                new (){ Description="IIS（pfx文件）",ServiceType="iis"},
                new (){ Description="其他（pem文件、crt文件、key文件）",ServiceType="other"},
                new (){ Description="根证书下载（crt文件）",ServiceType="root"},
            };
            _secret = secret;
            _httpClient = new HttpClient();
        }

        private async Task DownloadCert(string serviceType)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = _secret.SecretId,
                    SecretKey = _secret.SecretKey
                };
                SslClient client = new SslClient(cred, "");
                DescribeDownloadCertificateUrlRequest req = new DescribeDownloadCertificateUrlRequest();

                req.CertificateId = CertificateId;
                req.ServiceType = serviceType;
                DescribeDownloadCertificateUrlResponse resp = client.DescribeDownloadCertificateUrlSync(req);
                var bytes = await _httpClient.GetByteArrayAsync(resp.DownloadCertificateUrl);
                await File.WriteAllBytesAsync($"C:\\{resp.DownloadFilename}", bytes);
                MessageBox.Show("证书已下载到C盘");

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "下载证书文件出错", MessageBoxButton.OK);
            }
        }




        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {

            CertificateId = parameters.GetValue<string>("CertificateId");
        }
    }
}
