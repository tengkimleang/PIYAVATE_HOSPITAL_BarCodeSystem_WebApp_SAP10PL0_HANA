using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Domain.Common
{
    public record JwtResponse(
        string Token="",
        string RefreshToken="",
        string ErrorCode = "",
        string ErrorMessage = ""
        );
}
