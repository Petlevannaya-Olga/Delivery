using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetUncompletedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;
using Courier = OpenApi.Models.Courier;
using Location = OpenApi.Models.Location;

namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController(IMediator mediator) : DefaultApiController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public override async Task<IActionResult> CreateOrder()
    {
        var orderId = Guid.NewGuid();
        const string street = "Несуществующая";
        const int volume = 5;
        var createOrderCommand = CreateOrderCommand.Create(orderId, street, volume).Value;
        var response = await _mediator.Send(createOrderCommand);

        if (response.IsSuccess) 
            return Ok();

        return Conflict(response.Error);
    }

    public override async Task<IActionResult> GetCouriers()
    {
        var getAllCouriersQuery = new GetAllCouriersQuery();
        var response = await _mediator.Send(getAllCouriersQuery);
        
        if (response is null) 
            return NotFound();

        return Ok(response.Couriers.ToCouriers());
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getUncompletedOrdersQuery = new GetUncompletedOrdersQuery();
        var response = await _mediator.Send(getUncompletedOrdersQuery);

        if (response is null) 
            return NotFound();

        return Ok(response.Orders.ToOrders());
    }
}

file static class MapperExtensions
{
    public static List<Order> ToOrders(this List<OrderDto> ordersDto)
    {
        var model = ordersDto.Select(orderDto => new Order
        {
            Id = orderDto.Id,
            Location = new Location
            {
                X = orderDto.LocationDto.X,
                Y = orderDto.LocationDto.Y
            },
        });
        return model.ToList();
    }
    
    public static List<Courier> ToCouriers(this List<CourierDto> couriersDto)
    {
        var couriersModel = couriersDto.Select(courierDto => new Courier
        {
            Id = courierDto.Id,
            Name = courierDto.Name,
            Location = new Location
            {
                X = courierDto.LocationDto.X,
                Y = courierDto.LocationDto.Y
            }
        });
        return couriersModel.ToList();
    }
}