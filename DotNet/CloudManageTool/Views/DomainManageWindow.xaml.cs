using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CloudManageTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DomainManageWindow : Window
    {
        public DomainManageWindow()
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

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox box)
            {
                if (string.IsNullOrEmpty(box.Text))
                    box.Background = (ImageBrush)FindResource("watermark");
                else
                    box.Background = null;
            }
        }
    }
}
