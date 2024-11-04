using Microsoft.FluentUI.AspNetCore.Components;
using Refit;
using Piyavate_Hospital.Shared.Models;

namespace Piyavate_Hospital.Shared.Services;
public static class ErrorHandlingHelper
{
    public static async Task ExecuteWithHandlingAsync(Func<Task> action, PostResponse postResponse, IToastService toastService)
    {
        try
        {
            await action();
        }
        catch (ApiException ex)
        {
            if (postResponse.ErrorCode == "")
            {
                toastService.ShowError(ex.Content ?? "");
            }
            else
            {
                toastService.ShowError(postResponse.ErrorMsg);
            }
        }
    }

}
