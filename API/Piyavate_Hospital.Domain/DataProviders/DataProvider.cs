

namespace Piyavate_Hospital.Domain.DataProviders
{
    public record DataProvider(
        string StoreName = "",
        string DBType = "",
        string Par1 = "",
        string Par2 = "",
        string Par3 = "",
        string Par4 = "",
        string Par5 = "");
}
