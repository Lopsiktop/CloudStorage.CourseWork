using System.Collections.ObjectModel;

namespace CloudStorage.Client.Utils;

public static class ExtensionsUtils
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
    {
        var collection = new ObservableCollection<T>();
        foreach (var item in items)
            collection.Add(item);

        return collection;
    }
}
