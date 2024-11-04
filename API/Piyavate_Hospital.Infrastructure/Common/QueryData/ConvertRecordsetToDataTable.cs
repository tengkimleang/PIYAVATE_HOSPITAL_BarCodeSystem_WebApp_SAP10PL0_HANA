using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piyavate_Hospital.Application.Common.Interfaces.Setting;

namespace Piyavate_Hospital.Infrastructure.Common.QueryData
{
    internal class ConvertRecordsetToDataTable : IConvertRecordsetToDataTable
    {
        public DataTable ToDataTable(Recordset recordset)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < recordset.Fields.Count; i++)
            {
                dataTable.Columns.Add(recordset.Fields.Item(i).Name);
            }
            while (!recordset.EoF)
            {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < recordset.Fields.Count; i++)
                {
                    row[i] = recordset.Fields.Item(i).Value;
                }
                dataTable.Rows.Add(row);
                recordset.MoveNext();
            }
            return dataTable;
        }
    }
}
