using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piyavate_Hospital.Domain.Common;

public record ConnectionSettings
{
    public const string SectionName = "ConnectionStrings";
    public int DbServerType { get; init; }
    public string Server { get; init; }= string.Empty;
    public string LicenseServer { get; init; } = string.Empty;
    public string SLDServer { get; init; } = string.Empty;
    public string CompanyDB { get; init; } = string.Empty;
    public string UserNameSAP { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DbUserName { get; init; } = string.Empty;
    public string DbPassword { get; init; } = string.Empty;
    public bool UseTrusted { get; init; }
}
