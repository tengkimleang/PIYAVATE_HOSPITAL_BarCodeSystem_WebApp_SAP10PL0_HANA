using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.Layout;

public record LayoutCommand : IRequest<PrintViewLayoutResponse>
{
    public string? LayoutCode { get; init; }
    public string? DocEntry { get; init; }
    public string? StoreName { get; init; }
    public string? Path { get; set; }
}