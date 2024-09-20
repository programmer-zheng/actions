using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Documents;

namespace DomainManageTool.ViewModels;

public class CreateDomainRecordWindowViewModel : BindableBase
{
    private List<string> _recordTypeList;

    public List<string> RecordTypeList
    {
        get => _recordTypeList;
        set => SetProperty(ref _recordTypeList, value);
    }

    public CreateDomainRecordWindowViewModel()
    {
        RecordTypeList = new List<string>() { "A", "CNAME", "TXT" };
    }
}