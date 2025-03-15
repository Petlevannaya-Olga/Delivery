using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;

namespace DeliveryApp.Core.Ports;

/// <summary>
/// Репозиторий для Aggregate Courier
/// </summary>
public interface ICourierRepository
{
	/// <summary>
	/// Добавить курьера
	/// </summary>
	/// <param name="courier">Курьер</param>
	Task Add(Courier courier);

	/// <summary>
	/// Обновить курьера
	/// </summary>
	/// <param name="courier">Курьер</param>
	void Update(Courier courier);

	/// <summary>
	/// Получить курьера по идентификатору
	/// </summary>
	/// <param name="courierId">Идентификатор</param>
	Task<Maybe<Courier>> Get(Guid courierId);

	/// <summary>
	/// Получить всех свободных курьеров
	/// </summary>
	IEnumerable<Courier> GetAllFree();
}