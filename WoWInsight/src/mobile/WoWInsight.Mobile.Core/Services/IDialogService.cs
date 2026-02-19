using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface IDialogService
{
    Task DisplayAlertAsync(string title, string message, string cancel);
}
