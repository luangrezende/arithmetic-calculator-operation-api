using System.Reflection;

namespace ArithmeticCalculatorOperationApi.Application.Helpers
{
    public static class DatabaseScriptsHelper
    {
        public static string GetSqlScript(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Scripts.{name}.sql";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Resource {resourceName} not found.");
            
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
