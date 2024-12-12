using CloudStorage.Client.Models;
using CloudStorage.Client.UI.UIHelpers;
using CloudStorage.Client.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CloudStorage.Client.ViewModels;

public class HistoryViewModel : ObservableObject
{
    public ObservableCollection<ReturnHistoryDto> History { get; set; } = new ObservableCollection<ReturnHistoryDto>();

    public async Task Load()
    {
        History.Clear();

        var all = await ApiHelper.GetAllHistory();
        if (all.IsError)
        {
            await Notify.ShowAsync("Ошибка", all.Error, NotifyType.Error);
            return;
        }

        foreach (var item in all.Value)
            History.Add(item);
    }
}
