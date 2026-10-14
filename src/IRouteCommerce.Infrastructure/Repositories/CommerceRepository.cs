using System.Data;
using System.Text.Json;
using IRouteCommerce.Application.DTOs;
using IRouteCommerce.Application.Interfaces;
using IRouteCommerce.Domain.Entities;
using IRouteCommerce.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IRouteCommerce.Infrastructure.Repositories;

/// <summary>
/// Implementacion del repositorio de comercio. Invoca los stored procedures
/// definidos en scripts/init-db.sql a traves de EF Core.
/// </summary>
public class CommerceRepository : ICommerceRepository
{
    private readonly CommerceDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public CommerceRepository(CommerceDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateBulkAsync(IReadOnlyList<CommerceRowDto> rows, CancellationToken cancellationToken = default)
    {
        // Las filas se envian como JSON y el SP las inserta con OPENJSON.
        var json = JsonSerializer.Serialize(rows, JsonOptions);

        var jsonParameter = new SqlParameter("@Json", SqlDbType.NVarChar, -1) { Value = json };
        var outputParameter = new SqlParameter("@RegistrosCargados", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_create_commerce @Json, @RegistrosCargados OUTPUT",
            new object[] { jsonParameter, outputParameter },
            cancellationToken);

        return (int)(outputParameter.Value ?? 0);
    }

    public async Task<int> ProcessAsync(DateOnly processDate, CancellationToken cancellationToken = default)
    {
        var dateParameter = new SqlParameter("@pc_processdate", SqlDbType.Date)
        {
            Value = processDate.ToDateTime(TimeOnly.MinValue)
        };
        var outputParameter = new SqlParameter("@RegistrosEnCuarentena", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.sp_process_commerce @pc_processdate, @RegistrosEnCuarentena OUTPUT",
            new object[] { dateParameter, outputParameter },
            cancellationToken);

        return (int)(outputParameter.Value ?? 0);
    }

    public async Task<IReadOnlyList<CommerceQuarantine>> GetQuarantineAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CommerceQuarantines
            .FromSqlRaw("EXEC dbo.sp_get_quarantine")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
