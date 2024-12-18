using System.Windows;
using System.Windows.Input;

namespace DomainManageTool.Views
{
    /// <summary>
    /// SslManageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SslManageWindow : Window
    {
        public SslManageWindow()
        {
            InitializeComponent();
            BtnMin.Click += (s, e) => { WindowState = WindowState.Minimized; };
            BtnMax.Click += (s, e) =>
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else WindowState = WindowState.Maximized;
            };
            BtnClose.Click += (s, e) => { Close(); };

            TitleZone.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };


            TitleZone.MouseDoubleClick += (s, e) =>
            {

                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else WindowState = WindowState.Maximized;

            };
        }
    }
}
