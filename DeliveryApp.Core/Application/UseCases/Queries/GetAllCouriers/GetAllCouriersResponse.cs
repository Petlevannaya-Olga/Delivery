namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersResponse
{
    public GetAllCouriersResponse(List<CourierDto> couriers)
    {
        Couriers.AddRange(couriers);
    }

    public List<CourierDto> Couriers { get; set; } = new();
}

public class CourierDto
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public LocationDto LocationDto { get; set; }
}

public class LocationDto
{
    /// <summary>
    ///     Горизонталь
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     Вертикаль
    /// </summary>
    public int Y { get; set; }
}
