

using SAPbobsCOM;

namespace Piyavate_Hospital.Application.Common.Interfaces.Setting;

public interface IConnection
{
    Company Connect();
    void Disconnect();
}
