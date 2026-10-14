using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Domain.Entities;

namespace IRouteCommerce.Tests.Fakes;

/// <summary>Doble de prueba en memoria de ICommerceRepository.</summary>
internal sealed class FakeCommerceRepository : ICommerceRepository
{
    public int CreateBulkResult { get; set; }

    public int ProcessResult { get; set; }

    public IReadOnlyList<CommerceQuarantine> QuarantineResult { get; set; } = Array.Empty<CommerceQuarantine>();

    public IReadOnlyList<CommerceRowDto>? LastInsertedRows { get; private set; }

    public DateOnly? LastProcessedDate { get; private set; }

    public Task<int> CreateBulkAsync(IReadOnlyList<CommerceRowDto> rows, CancellationToken cancellationToken = default)
    {
        LastInsertedRows = rows;
        return Task.FromResult(CreateBulkResult);
    }

    public Task<int> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken = default)
    {
        LastProcessedDate = processDate;
        return Task.FromResult(ProcessResult);
    }

    public Task<IReadOnlyList<CommerceQuarantine>> GetQuarantineAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(QuarantineResult);
}
