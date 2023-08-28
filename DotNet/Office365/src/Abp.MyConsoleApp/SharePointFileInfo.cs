using Microsoft.Graph.Models;

namespace Abp.MyConsoleApp;

public class SharePointFileInfo
{
    public DriveItem FileInfo { get; set; }

    public string FileFullPath { get; set; }

    public string  DriveId { get; set; }
    
    public string DriveItemId { get; set; }
}