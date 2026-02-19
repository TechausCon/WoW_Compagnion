using System;
using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface IBrowserService
{
    Task OpenAsync(string url);
}
