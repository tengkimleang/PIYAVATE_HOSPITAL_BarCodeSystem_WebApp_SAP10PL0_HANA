using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Authorize
{
    public class AuthorizeCommand : IRequest<ErrorOr<JwtResponse>>
    {
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
