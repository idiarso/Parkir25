using System.Threading.Tasks;

namespace ParkIRC.Data.Services
{
    public interface ISiteSettingsService
    {
        Task<string> GetSiteSettingAsync(string key);
        Task SetSiteSettingAsync(string key, string value);
    }
}
