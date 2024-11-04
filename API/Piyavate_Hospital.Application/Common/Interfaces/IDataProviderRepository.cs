
using System.Data;
using Piyavate_Hospital.Domain.DataProviders;

namespace Piyavate_Hospital.Application.Common.Interfaces;

public interface IDataProviderRepository
{
    Task<DataTable> Query(DataProvider dataProviderRequest);
}
