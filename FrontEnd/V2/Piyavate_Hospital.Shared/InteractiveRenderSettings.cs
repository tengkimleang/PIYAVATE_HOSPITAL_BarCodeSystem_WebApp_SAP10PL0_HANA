using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Piyavate_Hospital.Shared
{
    public static class InteractiveRenderSettings
    {
        public static IComponentRenderMode InteractiveServer { get; set; } = RenderMode.InteractiveServer;
        public static IComponentRenderMode InteractiveAuto { get; set; } = RenderMode.InteractiveAuto;
        public static IComponentRenderMode InteractiveWebAssembly { get; set; } = RenderMode.InteractiveWebAssembly;

        public static void ConfigureBlazorHybridRenderModes()
        {
            InteractiveServer = RenderMode.InteractiveServer;
            InteractiveAuto = RenderMode.InteractiveAuto;
            InteractiveWebAssembly = RenderMode.InteractiveWebAssembly;
        }
    }
}
