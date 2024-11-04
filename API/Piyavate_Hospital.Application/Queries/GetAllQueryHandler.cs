using ErrorOr;
using MediatR;

using System.Data;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Domain.DataProviders;

namespace Piyavate_Hospital.Application.Queries;

public record GetAllQuery(
        string StoreName = "",
        string DBType = "",
        string Par1 = "",
        string Par2 = "",
        string Par3 = "",
        string Par4 = "",
        string Par5 = "") : IRequest<ErrorOr<DataTable>>;
public class GetAllQueryHandler : IRequestHandler<GetAllQuery, ErrorOr<DataTable>>
{
    IDataProviderRepository _dataProviderRepository;

    public GetAllQueryHandler(IDataProviderRepository dataProviderRepository)
    {
        _dataProviderRepository = dataProviderRepository;
    }

    public async Task<ErrorOr<DataTable>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        return await _dataProviderRepository.Query(new DataProvider
        {
            StoreName = request.StoreName,
            DBType = request.DBType,
            Par1 = request.Par1,
            Par2 = request.Par2,
            Par3 = request.Par3,
            Par4 = request.Par4,
            Par5 = request.Par5
        });
    }
}
