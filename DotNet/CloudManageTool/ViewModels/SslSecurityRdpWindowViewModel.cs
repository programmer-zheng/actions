using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManageTool.ViewModels
{
    public class SslSecurityRdpWindowViewModel : BindableBase, IDialogAware
    {

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public DialogCloseListener RequestClose { get; }

        public DelegateCommand CancelCommand { get; }

        public SslSecurityRdpWindowViewModel()
        {
            CancelCommand = new DelegateCommand(() => { RequestClose.Invoke(ButtonResult.Cancel); });
            Description = @"1、修改组策略
“gpedit.msc”打开组策略
依次展开“计算机配置”-“管理模板”-“Windows组件”-“远程桌面服务”-“远程桌面会话主机”-“安全”
将“设置客户端连接加密级别”设置为“已启用”，“加密级别”设置为“高”。
将“远程（RDP)连接要求使用指定的安全层”设置为“已启用”，“安全层”设置为“SSL”。

2、禁止 RDP 使用 TLS 1.0
打开注册表编辑器，找到路径：HKEY_LOCAL_MACHINE\SYSTEM\CurrentcontrolSet\Control\SecurityProviders\SCHANNEL\Protocols
鼠标右键点击 Protocols 项，选择 新建 - 项，命名新项为 TLS 1.0
鼠标右键点击 TLS 1.0 项，选择 新建 - 项，命名新项为 Server
鼠标右键点击 TLS 1.0 项，选择 新建 - 项，命名新项为 Client
鼠标分另右键点击 Server 、Client 项，选择 新建 - DWORD(32位)值，命名为 Enabled，双击 Enabled，将数值数据改为 0

3、配置证书
下载用于 Internet Information Services（IIS）的 PFX 格式 SSL 证书
双击 PFX 证书文件，开启“证书导入向导”，存储位置选择“本地计算机”，存储位置选择“将所有证书存储在下列存储中”，选择“个人”。

打开“本地计算机证书管理器”。运行（快捷键： Win + R）中输入 certlm.msc 来管理本地计算机证书。

选择并打开证书。选择“证书“>”本地计算机”>“个人”>“证书”，双击打开安装好的 SSL 证书。
查看证书指纹。在弹出的“证书”窗口中，选择“详细信息”标签页，就可以查看到证书指纹，将其复制。

PowerShell中输入以下命令
wmic /namespace:\\root\cimv2\TerminalServices PATH Win32_TSGeneralSetting Set SSLCertificateSHA1Hash=""复制的证书指纹""


参考链接：https://vickey.fun/2022/08/27/Make-Remote-Desktop-Connection-More-Secure
";
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
        }
    }
}
