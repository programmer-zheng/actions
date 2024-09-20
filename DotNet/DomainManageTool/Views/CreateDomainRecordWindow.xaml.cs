using System.Windows;
using System.Windows.Input;

namespace DomainManageTool.Views;

public partial class CreateDomainRecordWindow : Window
{
    public CreateDomainRecordWindow()
    {
        InitializeComponent();
        
        BtnClose.Click += (s, e) => { Close(); };
        
        
        TitleZone.MouseMove += (s, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        };
    }
}