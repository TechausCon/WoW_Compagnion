using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WoWInsight.Mobile.Services;

public class DialogService : IDialogService
{
    public Task DisplayAlertAsync(string title, string message, string cancel)
    {
        if (Shell.Current != null)
        {
            return Shell.Current.DisplayAlert(title, message, cancel);
        }
        return Task.CompletedTask;
    }
}
