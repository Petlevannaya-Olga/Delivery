using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersQueryHandler: IRequestHandler<GetAllCouriersQuery, GetAllCouriersResponse>
{
    private readonly string _connectionString;

    public GetAllCouriersQueryHandler(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        _connectionString = connectionString;
    }
    
    public async Task<GetAllCouriersResponse> Handle(GetAllCouriersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        var query =
            @"SELECT id as Id,
                name as Name,
                status_id as StatusId,
                transport_id as TransportId,
                location_x as X,
                location_y as Y
            FROM public.couriers";
        
        var couriers = await connection.QueryAsync<Courier, Location, Courier>(query, (courier, location) => {
                courier.Location = location;
                return courier;
            },
            new { },
            splitOn: "X" );

        return new GetAllCouriersResponse(couriers.ToList());
    }
}