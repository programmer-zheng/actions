using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CloudManageTool
{
    public class CustomDialogWindow : Window, IDialogWindow
    {
        public CustomDialogWindow()
        {
            this.WindowStyle = WindowStyle.None;  // 隐藏标题栏
            this.ResizeMode = ResizeMode.NoResize;  // 禁止调整大小
            this.SizeToContent = SizeToContent.WidthAndHeight;  // 自动调整大小
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;  // 居中显示
        }

        // 实现 IDialogWindow 接口
        public IDialogResult Result { get; set; }
    }

}
