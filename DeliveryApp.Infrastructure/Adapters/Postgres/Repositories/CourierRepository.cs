using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository(ApplicationDbContext dbContext) : ICourierRepository
{
	private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	public async Task Add(Courier courier)
	{
		await _dbContext.Couriers.AddAsync(courier);
	}

	public IEnumerable<Courier> GetAllFree()
	{
		var couriers = _dbContext
			.Couriers
			.Include(x => x.Transport)
			.Where(o => o.Status.Name == CourierStatus.Free.Name);
		return couriers;
	}

	public async Task<Maybe<Courier>> Get(Guid courierId)
	{
		var courier = await _dbContext
			.Couriers
			.Include(x => x.Transport)
			.FirstOrDefaultAsync(o => o.Id == courierId);
		return courier;
	}

	public void Update(Courier courier)
	{
		_dbContext.Couriers.Update(courier);
	}
}