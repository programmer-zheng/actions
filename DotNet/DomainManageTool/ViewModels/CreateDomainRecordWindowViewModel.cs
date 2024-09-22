using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System.Collections.Generic;
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

    public DelegateCommand SaveDomainRecordCommand { get; private set; }

    public CreateDomainRecordWindowViewModel()
    {
        RecordTypeList = new List<string>() { "A", "CNAME", "TXT" };

        SaveDomainRecordCommand = new DelegateCommand(() =>
        {
            RequestClose.Invoke(ButtonResult.OK);
        });
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