namespace DeliveryApp.Core.Application.UseCases.CommonDtos;

public class LocationDto(int x, int y)
{
    /// <summary>
    /// Координата по оси X
    /// </summary>
    public int X { get; set; } = x;

    /// <summary>
    /// Координата по оси Y
    /// </summary>
    public int Y { get; set; } = y;
}