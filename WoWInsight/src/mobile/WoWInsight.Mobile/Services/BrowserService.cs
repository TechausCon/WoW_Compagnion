using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace WoWInsight.Mobile.Services;

public class BrowserService : IBrowserService
{
    public async Task OpenAsync(string url)
    {
        await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
    }
}
