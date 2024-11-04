using Piyavate_Hospital.Shared.Services;

namespace Piyavate_Hospital.WebApp.Server.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }
        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
