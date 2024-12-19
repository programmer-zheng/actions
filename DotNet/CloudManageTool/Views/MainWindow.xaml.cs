using System.Windows;
using System.Windows.Input;

namespace CloudManageTool.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BtnMin.Click += (s, e) => { WindowState = WindowState.Minimized; };
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
