
using SAPbobsCOM;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Application.Common.Interfaces.Setting;
using Piyavate_Hospital.Domain.DataProviders;

namespace Piyavate_Hospital.Infrastructure.Common.QueryData;

public class DataProviderRepository : IDataProviderRepository
{
    private readonly IConnection IConnection;
    private readonly IConvertRecordsetToDataTable convertRecordsetToDataTable;
    public DataProviderRepository(IConnection iConnection, IConvertRecordsetToDataTable convertRecordsetToDataTable)
    {
        IConnection = iConnection;
        this.convertRecordsetToDataTable = convertRecordsetToDataTable;
    }
    public Task<DataTable> Query(DataProvider dataProviderRequest)
    {
        var connection = IConnection.Connect();
        Recordset recordset = (Recordset)connection.GetBusinessObject(BoObjectTypes.BoRecordset);
        string query = connection.DbServerType == BoDataServerTypes.dst_HANADB
            ? $"CALL \"{connection.CompanyDB}\".\"{dataProviderRequest.StoreName}\" ('{dataProviderRequest.DBType}','{dataProviderRequest.Par1}','{dataProviderRequest.Par2}','{dataProviderRequest.Par3}','{dataProviderRequest.Par4}','{dataProviderRequest.Par5}')"
            : $"EXEC \"{connection.CompanyDB}\".\"{dataProviderRequest.StoreName}\" ('{dataProviderRequest.DBType} ',' {dataProviderRequest.Par1} ',' {dataProviderRequest.Par2} ',' {dataProviderRequest.Par3} ',' {dataProviderRequest.Par4} ',' {dataProviderRequest.Par5}')";

        recordset.DoQuery(query);

        return Task.FromResult(convertRecordsetToDataTable.ToDataTable(recordset));
    }
}
