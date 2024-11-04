
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Common.Interfaces;

public interface IReportLayout
{
    Task<PrintViewLayoutResponse> CallViewLayout(string code, string docEntry, string path,string storeName);
}
