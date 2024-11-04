using Microsoft.Extensions.Hosting;
using System.Collections.ObjectModel;
using Piyavate_Hospital.Shared.Models;

namespace Piyavate_Hospital.Shared.Services;

public class LoadMasterDataService : BackgroundService
{
    private readonly ILoadMasterData loadMasterData;
    public LoadMasterDataService(ILoadMasterData loadMasterData)
    {
        this.loadMasterData = loadMasterData;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await loadMasterData.LoadItemMaster();
        await loadMasterData.LoadVendorMaster();
        await loadMasterData.LoadContactPersonMaster();
        await loadMasterData.LoadGetTaxPurchaseMaster();
        await loadMasterData.LoadGetWarehouseMaster();
    }
}
