using DeliveryApp.Core.Application.UseCases.CommonDtos;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetUncompletedOrders;

public class GetUncompletedOrdersResponse
{
    public GetUncompletedOrdersResponse(List<OrderDto> orders)
    {
        Orders.AddRange(orders);
    }

    public List<OrderDto> Orders { get; set; } = new();
}

public class OrderDto(Guid id, LocationDto locationDto)
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; } = id;

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public LocationDto LocationDto { get; set; } = locationDto;
}