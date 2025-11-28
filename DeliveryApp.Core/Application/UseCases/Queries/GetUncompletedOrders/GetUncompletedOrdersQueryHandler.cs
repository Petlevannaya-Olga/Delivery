using Dapper;
using DeliveryApp.Core.Application.UseCases.CommonDtos;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetUncompletedOrders;

public class GetUncompletedOrdersQueryHandler: IRequestHandler<GetUncompletedOrdersQuery, GetUncompletedOrdersResponse>
{
    private readonly string _connectionString;
    public GetUncompletedOrdersQueryHandler(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        _connectionString = connectionString;
    }
    
    public async Task<GetUncompletedOrdersResponse> Handle(GetUncompletedOrdersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var query = @"Select id, location_x, location_y from public.orders where order_status <> 'completed'";

        var result = await connection.QueryAsync<OrderDto, LocationDto, OrderDto>
        (
            query,
            (order, location) =>
            {
                order.LocationDto = location;
                return order;
            },
            splitOn: "X",
            param: new { statuses = new[] { OrderStatus.Created.Name, OrderStatus.Assigned.Name } }
        );
        
        return new GetUncompletedOrdersResponse(result.ToList());
    }
}