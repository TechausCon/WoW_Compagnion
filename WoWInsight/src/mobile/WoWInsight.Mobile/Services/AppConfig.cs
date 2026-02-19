namespace WoWInsight.Mobile.Services;

public class AppConfig : IAppConfig
{
    public string ApiBaseUrl { get; }

    public AppConfig()
    {
        // For Android emulator vs iOS simulator vs device
        if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android)
            ApiBaseUrl = "https://10.0.2.2:7123";
        else
            ApiBaseUrl = "https://localhost:7123";
    }
}
