using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Infrastructure.Common.Setting;

public record ReportBodyResponse(
    string TypeOfParameter, 
    string DataSetName,
    List<SubReportBody> ListOfSubReportBody);

public record SubReportBody(
    string SubReportNamePath,
    string DataSetName,
    string SubTypeOfParameter);

#region Sample Json Request
//    [
//	{
//		"TypeOfParameter":"STORETYPE",
//		"DataSetName":"DataSource"
//		"SubReportSource":

//                [
//					{
//						"SubReportNamePath": "",//Name Of SubReport
//						"DataSetName":"",
//						"SubTypeOfParameter":""

//                    }

//				]
//	},
//	{
//    "TypeOfParameter":"STORETYPE",
//		"DataSetName":"DataSource"

//        "SubReportSource"::[]

//    }
//]
#endregion