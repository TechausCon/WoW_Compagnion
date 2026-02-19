using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WoWInsight.Mobile.Services;

public class NavigationService : INavigationService
{
    public Task GoToAsync(string route)
    {
        if (Shell.Current != null)
        {
            return Shell.Current.GoToAsync(route);
        }
        return Task.CompletedTask;
    }

    public Task GoToAsync(string route, IDictionary<string, object> parameters)
    {
        if (Shell.Current != null)
        {
            return Shell.Current.GoToAsync(route, parameters);
        }
        return Task.CompletedTask;
    }
}
