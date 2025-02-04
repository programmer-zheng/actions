using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using CloudManageTool.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
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

        public AsyncDelegateCommand<string> ImportCertCommand { get; }

        private string _certFileFullPath;

        private const string CertSavePath = @"C:\";


        public SslDownloadWindowViewModel(PlatFormSecret secret)
        {
            CancelCommand = new DelegateCommand(() => { RequestClose.Invoke(ButtonResult.Cancel); });

            DownloadCertCommand = new AsyncDelegateCommand<string>(DownloadCert);

            ImportCertCommand = new AsyncDelegateCommand<string>(ImportCert);

            // nginx tomcat apache iis jks other root
            DownloadList = new List<SslDownloadDto>
            {
                new() { Description = "Tomcat（pfx格式）", ServiceType = "tomcat" },
                new() { Description = "Tomcat（JKS格式）", ServiceType = "jks" },
                new() { Description = "Apache（crt文件、key文件）", ServiceType = "apache" },
                new() { Description = "Nginx（适用大部分场景）（pem文件、crt文件、key文件）", ServiceType = "nginx" },
                new() { Description = "IIS（pfx文件）", ServiceType = "iis" },
                new() { Description = "其他（pem文件、crt文件、key文件）", ServiceType = "other" },
                new() { Description = "根证书下载（crt文件）", ServiceType = "root" },
            };
            _secret = secret;
            _httpClient = new HttpClient();
        }

        private X509Certificate2 ImportPfxCertificate(string pfxFilePath, string password)
        {
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(pfxFilePath, password);
            return certificate;
        }

        /// <summary>
        /// 导入证书到证书存储
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        private void ImportCertificateToStore(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            using (X509Store store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
            }
        }

        private Task ImportCert(string serviceType)
        {
            if (!serviceType.Equals("iis", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("目前仅支持IIS证书");
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(_certFileFullPath))
            {
                MessageBox.Show("请先下载证书文件");
                return Task.CompletedTask;
            }

            if (!File.Exists(_certFileFullPath))
            {
                MessageBox.Show($"{_certFileFullPath} 证书文件不存在，请检查或重新下载");
                return Task.CompletedTask;
            }

            try
            {
                var (pfxPath, password) = UnzipcertificateFileReturnCertificatePathAndPassword(_certFileFullPath);
                // 导入证书
                var cert = ImportPfxCertificate(pfxPath, password);
                if (cert != null)
                {
                    ImportCertificateToStore(cert);

                    MessageBox.Show($"证书导入成功");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "导入证书出错", MessageBoxButton.OK);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 解压证书文件并返回证书路径和密码
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <returns></returns>
        private Tuple<string, string> UnzipcertificateFileReturnCertificatePathAndPassword(string zipFilePath)
        {
            var extension = Path.GetExtension(zipFilePath);
            var dirPath = zipFilePath.Replace(extension, "");
            string password;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Name))
                    {
                        string destinationPath = Path.Combine(dirPath, entry.Name);
                        entry.ExtractToFile(destinationPath, overwrite: true);
                    }
                }
            }

            var files = Directory.GetFiles(dirPath);
            var txtFilePath = files.First(t => t.EndsWith("txt"));
            using (var stream = File.OpenRead(txtFilePath))
            using (var reader = new StreamReader(stream))
            {
                password = reader.ReadToEnd();
            }

            var certPath = files.First(t => !t.EndsWith("txt"));

            return new Tuple<string, string>(certPath, password);
        }

        /// <summary>
        /// 下载证书
        /// </summary>
        /// <param name="serviceType"></param>
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
                _certFileFullPath = Path.Combine(CertSavePath, resp.DownloadFilename);
                await File.WriteAllBytesAsync(_certFileFullPath, bytes);
                MessageBox.Show($"证书已下载到{CertSavePath}");
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