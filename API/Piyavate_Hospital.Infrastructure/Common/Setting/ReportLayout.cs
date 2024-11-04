using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using System.Data;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Domain.Common;
using Piyavate_Hospital.Domain.DataProviders;

namespace Piyavate_Hospital.Infrastructure.Common.Setting;

public class ReportLayout(IDataProviderRepository dataProviderRepository) : IReportLayout
{
    public async Task<PrintViewLayoutResponse> CallViewLayout(string code, string docEntry, string path, string storeName)
    {
        try
        {
            var reportSetup = dataProviderRepository.Query(new DataProvider(storeName, "CallLayout", code)).Result;
            var type = GetTypeExport(reportSetup.Rows[0]["EXPORTTYPE"].ToString() ?? "");
            LocalReport lr = new LocalReport();
            Stream reportDefinition = File.OpenRead($"{path}\\Report\\{reportSetup.Rows[0]["FILENAME"]}");
            lr.LoadReportDefinition(reportDefinition);
            lr.EnableExternalImages = true; // Enable external images
            lr.EnableHyperlinks = true; // Enable hyperlinks
            foreach (var a in JsonConvert.DeserializeObject<List<ReportBodyResponse>>(reportSetup.Rows[0]["PROPERTIES"].ToString()!)!)
            {
                DataTable dt = await dataProviderRepository.Query(new DataProvider(StoreName: reportSetup.Rows[0]["STOREPROCEDURE"].ToString() ?? "", DBType: a.TypeOfParameter, Par1: docEntry));
                lr.DataSources.Add(new ReportDataSource(a.DataSetName, dt));
            }
            lr.Refresh();
            var result = lr.Render(reportSetup.Rows[0]["EXPORTTYPE"].ToString()!);
            return new PrintViewLayoutResponse(
                ErrCode: "",
                ErrorMessage: "",
                Data: result,
                ApplicationType: type.Item2,
                FileName: reportSetup.Rows[0]["LAYOUTPRINTNAME"].ToString() ?? "");
        }
        catch (Exception e)
        {
            var a = e.Message;
            return new PrintViewLayoutResponse(
                ErrCode: "123",
                ErrorMessage: "123123",
                Data: [],
                ApplicationType: "",
                FileName: "");
        }
    }
    private Tuple<string, string, string> GetTypeExport(string type)
    {
        if (type == "PDF")
        {
            return Tuple.Create("PDF", "application/pdf", "");
        }
        else if (type == "WORD")
        {
            return Tuple.Create("WORD", "application/msword", "word.doc");
        }
        else if (type == "EXCEL")
        {
            return Tuple.Create("EXCEL", "application/xlsx", "excel.xls");
        }
        return Tuple.Create("PDF", "application/pdf", "");
    }
}
