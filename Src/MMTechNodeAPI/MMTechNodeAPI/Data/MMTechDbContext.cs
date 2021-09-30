using Microsoft.EntityFrameworkCore;
using MMTechNodeAPI.Models;

namespace MMTechNodeAPI.Data
{
    public class MMTechDbContext : DbContext
    {
        public MMTechDbContext(DbContextOptions<MMTechDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        
        public DbSet<Device> Devices { get; set; }
        public DbSet<Sensor> Sensors { get; set; } //created sensor table  
        
        public DbSet<SensorData> SensorData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.UserId);
            
            modelBuilder.Entity<Sensor>()  //With Entity Sensor
                .HasOne<Device>() //will have one device
                .WithMany() //connected to many sensors
                .HasForeignKey(s => s.DeviceId); //has foreign key from DeviceID

            modelBuilder.Entity<SensorData>()
                .HasOne<Sensor>()
                .WithMany()
                .HasForeignKey(sd => sd.SensorId);
        }
    }
}