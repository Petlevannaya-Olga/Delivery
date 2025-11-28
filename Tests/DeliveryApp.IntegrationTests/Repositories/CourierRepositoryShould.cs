using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using FluentAssertions;
using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.IntegrationTests.Repositories
{
	public class CourierRepositoryShould : IAsyncLifetime
	{
		/// <summary>
		/// Настройка Postgres из библиотеки TestContainers
		/// </summary>
		/// <remarks>По сути это Docker контейнер с Postgres</remarks>
		private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
			.WithImage("postgres:14.7")
			.WithDatabase("order")
			.WithUsername("username")
			.WithPassword("secret")
			.WithCleanUp(true)
			.Build();

		private ApplicationDbContext _context;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
		public CourierRepositoryShould()
		{
		}

		/// <summary>
		/// Инициализируем окружение
		/// </summary>
		/// <remarks>Вызывается перед каждым тестом</remarks>
		public async Task InitializeAsync()
		{
			//Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
			await _postgreSqlContainer.StartAsync();

			//Накатываем миграции и справочники
			var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
					_postgreSqlContainer.GetConnectionString(),
					sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
				.Options;
			_context = new ApplicationDbContext(contextOptions);
			_context.Database.Migrate();
		}

		/// <summary>
		/// Уничтожаем окружение
		/// </summary>
		/// <remarks>Вызывается после каждого теста</remarks>
		public async Task DisposeAsync()
		{
			await _postgreSqlContainer.DisposeAsync().AsTask();
		}

		[Fact]
		public async Task CanAddCourier()
		{
			//Arrange
			var courierCreateResult = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value);
			courierCreateResult.IsSuccess.Should().BeTrue();
			var courier = courierCreateResult.Value;

			//Act
			var courierRepository = new CourierRepository(_context);
			var unitOfWork = new UnitOfWork(_context);
			await courierRepository.Add(courier);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getCourierFromDbResult = await courierRepository.Get(courier.Id);
			getCourierFromDbResult.HasValue.Should().BeTrue();
			var courierFromDb = getCourierFromDbResult.Value;
			courier.Should().BeEquivalentTo(courierFromDb);
		}

		[Fact]
		public async Task CanUpdateCourier()
		{
			//Arrange
			var courierCreateResult = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value);
			courierCreateResult.IsSuccess.Should().BeTrue();
			var courier = courierCreateResult.Value;

			var courierRepository = new CourierRepository(_context);
			var unitOfWork = new UnitOfWork(_context);
			await courierRepository.Add(courier);
			await unitOfWork.SaveChangesAsync();

			//Act
			var courierStartWorkResult = courier.SetBusy();
			courierStartWorkResult.IsSuccess.Should().BeTrue();
			courierRepository.Update(courier);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getCourierFromDbResult = await courierRepository.Get(courier.Id);
			getCourierFromDbResult.HasValue.Should().BeTrue();
			var courierFromDb = getCourierFromDbResult.Value;
			courier.Should().BeEquivalentTo(courierFromDb);
			courierFromDb.Status.Should().Be(CourierStatus.Busy);
		}

		[Fact]
		public async Task CanGetById()
		{
			//Arrange
			var courierCreateResult = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value);
			courierCreateResult.IsSuccess.Should().BeTrue();
			var courier = courierCreateResult.Value;

			//Act
			var courierRepository = new CourierRepository(_context);
			var unitOfWork = new UnitOfWork(_context);
			await courierRepository.Add(courier);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getCourierFromDbResult = await courierRepository.Get(courier.Id);
			getCourierFromDbResult.HasValue.Should().BeTrue();
			var courierFromDb = getCourierFromDbResult.Value;
			courier.Should().BeEquivalentTo(courierFromDb);
		}

		[Fact]
		public async Task CanGetFirstInCreatedStatus()
		{
			//Arrange
			var courier1CreateResult = Courier.Create("Иван", Transport.Pedestrian, Location.Create(1, 1).Value);
			courier1CreateResult.IsSuccess.Should().BeTrue();
			var courier1 = courier1CreateResult.Value;
			courier1.SetBusy();

			var courier2CreateResult = Courier.Create("Борис", Transport.Pedestrian, Location.Create(1, 1).Value);
			courier2CreateResult.IsSuccess.Should().BeTrue();
			var courier2 = courier2CreateResult.Value;

			var courierRepository = new CourierRepository(_context);
			var unitOfWork = new UnitOfWork(_context);
			await courierRepository.Add(courier1);
			await courierRepository.Add(courier2);
			await unitOfWork.SaveChangesAsync();

			//Act
			var activeCouriersFromDb = courierRepository.GetAllFree();

			//Assert
			var couriersFromDb = activeCouriersFromDb.ToList();
			couriersFromDb.Should().NotBeEmpty();
			couriersFromDb.Count().Should().Be(1);
			couriersFromDb.First().Should().BeEquivalentTo(courier2);
		}
	}
}