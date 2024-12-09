using CloudStorage.Client.UI.Modals;
using CloudStorage.Client.Utils;

namespace CloudStorage.Client.UI.UIHelpers;

public static class Confirm
{
    public static bool ShowConfimation(string title, string yesbtn, string nobtn)
    {
        var conf = new Confirmation(title, yesbtn, nobtn);
        conf.Owner = WindowUtils.ActiveWindow;
        return conf.ShowDialog() ?? false;
    }
}
