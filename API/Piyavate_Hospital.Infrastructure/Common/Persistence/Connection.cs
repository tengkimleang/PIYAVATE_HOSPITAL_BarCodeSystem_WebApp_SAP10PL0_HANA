using Microsoft.Extensions.Options;
using SAPbobsCOM;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Application.Common.Interfaces.Setting;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Infrastructure.Common.Persistence;

public class Connection : IConnection, IUnitOfWork
{
    private readonly ConnectionSettings _settings;
    public static Company _company = new();
    public void Disconnect()
    {
        //cheecking coonection
        if (!_company.Connected) return;
        _company.Disconnect();
    }
    public Connection(IOptions<ConnectionSettings> settings)
    {
        _settings = settings.Value;
        Connect();
    }

    public void BeginTransaction(Company company)
    {
        if (company.InTransaction)
        {
            company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
        }
        company.StartTransaction();
    }

    public void Commit(Company company)
    {
        company.EndTransaction(BoWfTransOpt.wf_Commit);
    }

    public Company Connect()
    {
        //checking connection
        if (_company.Connected) return _company;
        //set value from setting to _company
        _company = new Company
        {
            Server = _settings.Server,
            CompanyDB = _settings.CompanyDB,
            UserName = _settings.UserNameSAP,
            Password = _settings.Password,
            DbUserName = _settings.DbUserName,
            DbPassword = _settings.DbPassword,
            UseTrusted = _settings.UseTrusted,
            // DbServerType 7 = dst_MSSQL2012,8 = dst_MSSQL2014,9 = dst_HANADB
            DbServerType = (BoDataServerTypes)_settings.DbServerType,
            SLDServer = _settings.SLDServer,
            LicenseServer = _settings.LicenseServer
        };
        var a=_company.Connect();
        if(a!= 0)
        {
            throw new System.Exception(_company.GetLastErrorDescription());
        }
        return _company;
    }
    public void Rollback(Company company)
    {
        if (company.InTransaction)
        {
            company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
        }
    }
}
