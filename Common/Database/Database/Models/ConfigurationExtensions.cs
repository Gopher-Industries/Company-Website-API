using Microsoft.Extensions.Configuration;
using System.Text;

namespace Database.Models
{
    public static class ConfigurationExtensions
    {

        public static string GetJson(this IConfiguration configurationManager, string Key)
        {

            var StrBuilder = new StringBuilder();
            StrBuilder.AppendLine("{");

            var Children = configurationManager.GetRequiredSection(Key).GetChildren();
            foreach (var ChildrenItem in Children)
                StrBuilder.AppendLine($"\"{ChildrenItem.Key}\": \"{ChildrenItem.Value}\",");

            StrBuilder.AppendLine("}");

            return StrBuilder.ToString();

            //var JsonObject = new JObject();
            //foreach (var child in configurationManager.GetRequiredSection(Key).GetChildren())
            //    JsonObject[child.Key] = child.Value;

            //return JsonObject.ToString();
        }

    }
}
