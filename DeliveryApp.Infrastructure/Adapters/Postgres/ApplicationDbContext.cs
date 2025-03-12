using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryApp.Infrastructure.Adapters.Postgres
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Order> Orders { get; set; }
		public DbSet<Courier> Couriers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Apply Configuration
			modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new CourierEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new TransportEntityTypeConfiguration());
		}
	}

}
