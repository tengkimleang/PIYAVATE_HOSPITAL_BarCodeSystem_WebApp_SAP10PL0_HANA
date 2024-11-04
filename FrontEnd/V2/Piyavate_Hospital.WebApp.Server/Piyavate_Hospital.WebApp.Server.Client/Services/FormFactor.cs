using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.WebApp.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "WebAssembly";
        }
        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
