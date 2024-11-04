using ErrorOr;
using MediatR;
using Piyavate_Hospital.Application.Common.Interfaces;
using Piyavate_Hospital.Application.Common.Interfaces.Setting;
using Piyavate_Hospital.Domain.Common;
using Piyavate_Hospital.Domain.DataProviders;

namespace Piyavate_Hospital.Application.Authorize
{
    public class AuthorizeCommandHandler(IJwtRegister jwtRegister, IDataProviderRepository dataProviderRepository)
        : IRequestHandler<AuthorizeCommand, ErrorOr<JwtResponse>>
    {
        public async Task<ErrorOr<JwtResponse>> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
        {
            var dt = await dataProviderRepository.Query(new DataProvider
            {
                StoreName = "_USP_CALLTRANS_EWTRANSACTION",
                DBType = "JwtCheckAccount",
                Par1 = request.Account,
                Par2 = request.Password
            });
            if (dt.Rows.Count == 0)
            {
                return await Task.FromResult(new JwtResponse("404", "Invalid User", "", "").ToErrorOr()).ConfigureAwait(false);
            }
            return await Task.FromResult((await jwtRegister
                .GenerateToken(request.Account)
                .ConfigureAwait(false))
                .ToErrorOr())
                .ConfigureAwait(false);
        }
    }
}
