
using System.Data;

namespace Piyavate_Hospital.Domain.DataProviders;

public record GetResponse(
    string ErrorCode = "",
    string ErrorMessage = "",
    DataTable Data = null!
    );
