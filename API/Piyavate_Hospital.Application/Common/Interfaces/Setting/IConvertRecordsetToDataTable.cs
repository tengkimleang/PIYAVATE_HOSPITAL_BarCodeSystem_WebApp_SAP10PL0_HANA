

using SAPbobsCOM;
using System.Data;

namespace Piyavate_Hospital.Application.Common.Interfaces.Setting;

public interface IConvertRecordsetToDataTable
{
    DataTable ToDataTable(Recordset recordset);
}
