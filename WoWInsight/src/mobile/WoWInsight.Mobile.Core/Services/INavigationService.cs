using System.Collections.Generic;
using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync(string route, IDictionary<string, object> parameters);
}
