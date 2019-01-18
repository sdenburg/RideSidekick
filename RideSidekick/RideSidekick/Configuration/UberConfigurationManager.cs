using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using RideSidekick.Pages;

namespace RideSidekick.Configuration
{
    public static class UberConfigurationManager
    {
        public static UberConfiguration Configuration { get; private set; }

        public static void LoadConfiguration()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(RideResultsListPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("RideSidekick.Configuration.UberConfiguration.json");
            using (var reader = new StreamReader(stream))
            {
                string fileContents = reader.ReadToEnd();
                UberConfigurationManager.Configuration = JsonConvert.DeserializeObject<UberConfiguration>(fileContents);
            }
        }
    }
}
