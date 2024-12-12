using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CloudStorage.Client.ViewModels;

public class HistoryViewModel : ObservableObject
{
    public ObservableCollection<ReturnHistoryDto> History { get; set; } = new ObservableCollection<ReturnHistoryDto>();

    public async Task Load(int? fileId = null, int? dirId = null)
    {
        History.Clear();

        Result<List<ReturnHistoryDto>> all = null;

        if(fileId != null)
        {
            all = await ApiHelper.GetFileHistory(fileId ?? 0);
        }
        else if(dirId != null)
        {
            all = await ApiHelper.GetFolderHistory(dirId ?? 0);
        }
        else if(fileId == null && dirId == null)
        {
            all = await ApiHelper.GetAllHistory();
        }

        if (all.IsError)
        {
            await Notify.ShowAsync("Ошибка", all.Error, NotifyType.Error);
            return;
        }

        foreach (var item in all.Value)
            History.Add(item);
    }
}
