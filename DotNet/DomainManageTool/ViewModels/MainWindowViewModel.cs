using CloudManageTool.Views;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

namespace CloudManageTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {


        public DelegateCommand GoToDomainManageWindowCommand { get; private set; }

        public DelegateCommand GoToSslManageWindowCommand { get; private set; }
        public IDialogService DialogService { get; }

        public MainWindowViewModel(IDialogService dialogService)
        {
            GoToDomainManageWindowCommand = new DelegateCommand(GoToDomainManageWindow);
            GoToSslManageWindowCommand = new DelegateCommand(GoToSslManageWindow);
            DialogService = dialogService;
        }

        private void GoToDomainManageWindow()
        {
            var newWindow = new DomainManageWindow();
            newWindow.Show();


        }
        private void GoToSslManageWindow()
        {
            var newWindow = new SslManageWindow();
            newWindow.Show();
        }
    }
}
