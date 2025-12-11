using Dapper;
using DeliveryApp.Core.Application.UseCases.CommonDtos;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetUncompletedOrders;

public class GetUncompletedOrdersQueryHandler : IRequestHandler<GetUncompletedOrdersQuery, GetUncompletedOrdersResponse>
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

    public async Task<GetUncompletedOrdersResponse> Handle(GetUncompletedOrdersQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var queryResult = await connection.QueryAsync<dynamic>(
            """
            SELECT o.id, o.location_x, o.location_y
                                FROM public.orders as o
                                where o.status != @completedStatus;
            """
            , new { completedStatus = OrderStatus.Completed.Name });
        
        var result = queryResult.ToList();

        if (result.Count == 0)
            return null;

        var orders = result.Select(order =>
        {
            var locationDto = new LocationDto(order.location_x, order.location_y);
            return new OrderDto(order.id, locationDto);
        }).ToList();
        
        return new GetUncompletedOrdersResponse(orders);
    }
}