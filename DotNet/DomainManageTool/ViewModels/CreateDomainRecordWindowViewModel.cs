using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DomainManageTool.ViewModels;

public class CreateDomainRecordWindowViewModel : BindableBase, IDialogAware
{
    public string Title { get; set; } = string.Empty;


    private List<string> _recordTypeList;

    public List<string> RecordTypeList
    {
        get => _recordTypeList;
        set => SetProperty(ref _recordTypeList, value);
    }

    public DialogCloseListener RequestClose { get; }

    public AsyncDelegateCommand SaveDomainRecordCommand { get; private set; }

    public DelegateCommand CancelCommand { get; private set; }

    public CreateDomainRecordWindowViewModel()
    {
        RecordTypeList = new List<string>() { "A", "CNAME", "TXT" };
        CancelCommand = new DelegateCommand(() =>
        {
            RequestClose.Invoke(ButtonResult.Cancel);
        });
        SaveDomainRecordCommand = new AsyncDelegateCommand(SaveRecord);
    }

    private bool Valid()
    {
        // TODO 验证输入
        return true;
    }

    private async Task SaveRecord()
    {
        if (Valid())
        {
            // TODO 添加记录逻辑
            RequestClose.Invoke(ButtonResult.OK);
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

    }
}