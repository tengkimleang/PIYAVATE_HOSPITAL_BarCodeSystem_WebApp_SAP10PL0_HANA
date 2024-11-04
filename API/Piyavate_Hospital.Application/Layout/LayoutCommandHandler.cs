using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.Interfaces;
using Throw;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Layout;

public class LayoutCommandHandler(IReportLayout reportLayout)
    : IRequestHandler<LayoutCommand, PrintViewLayoutResponse>
{
    public async Task<PrintViewLayoutResponse> Handle(LayoutCommand request, CancellationToken cancellationToken)
    {
        if (request is { LayoutCode: not null, DocEntry: not null, Path: not null, StoreName: not null })
        {
            var result = await reportLayout.CallViewLayout(request.LayoutCode, request.DocEntry, request.Path, request.StoreName);
            return new PrintViewLayoutResponse(
                ErrCode: result.ErrCode,
                ErrorMessage: result.ErrorMessage,
                FileName: result.FileName,
                ApplicationType: result.ApplicationType,
                Data: result.Data);
        }

        return new PrintViewLayoutResponse(ErrCode: "1111", ErrorMessage: "Null");
    }
}
