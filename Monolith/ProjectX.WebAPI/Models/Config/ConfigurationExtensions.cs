using System.Text;

namespace ProjectX.WebAPI.Models.Config
{
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Reads configuration into 
        /// </summary>
        /// <param name="configurationManager"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string GetJson(this IConfiguration configurationManager, string Key)
        {

            var StrBuilder = new StringBuilder();
            StrBuilder.AppendLine("{");

            var Children = configurationManager.GetRequiredSection(Key).GetChildren();
            foreach (var ChildrenItem in Children)
                StrBuilder.AppendLine($"\"{ChildrenItem.Key}\": \"{ChildrenItem.Value}\",");

            StrBuilder.AppendLine("}");

            return StrBuilder.ToString();

        }

    }
}
