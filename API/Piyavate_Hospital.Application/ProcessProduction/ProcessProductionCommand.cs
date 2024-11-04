using MediatR;
using ErrorOr;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.ProcessProduction;

public record ProcessProductionCommand(
    List<ProcessProductionLine> Data
) : IRequest<ErrorOr<PostResponse>>;

public record ProcessProductionLine(
    int ProductionNo,
    string ProcessStage,
    string Status
);