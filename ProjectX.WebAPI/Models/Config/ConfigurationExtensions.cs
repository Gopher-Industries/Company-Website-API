using Google.Cloud.SecretManager.V1;
using Newtonsoft.Json.Linq;

namespace ProjectX.WebAPI.Models.Config
{
    public static class ConfigurationExtensions
    {

        public static string GetJson(this IConfiguration configurationManager, string Key)
        {
            var JsonObject = new JObject();
            foreach (var child in configurationManager.GetRequiredSection(Key).GetChildren())
                JsonObject[child.Key] = child.Value;

            return JsonObject.ToString();
        }

    }
}
