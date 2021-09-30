using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMTechNodeAPI.Data;
using MMTechNodeAPI.Models;

namespace MMTechNodeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly MMTechDbContext _context;

        public DeviceController(MMTechDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(Guid id)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null) return NotFound();

            return device;
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<IEnumerable<Device>>> GetUserDevices(Guid id)
        {
            var devices = await _context.Devices
                .Where(d => d.UserId == id)
                .ToListAsync();

            return devices;
        }

        [HttpGet("data/{id}")]
        public async Task<ActionResult<object>> GetDeviceData(Guid id)
        {
            var dataList =
                await _context.SensorData.FromSqlRaw(
                    " select " +
                    "    s1.* " +
                    " from " +
                    "    \"SensorData\" s1, " +
                    "    ( " +
                    "        select \"SensorId\" as SensorId, max(\"TimeRecord\") as TimeRecord " +
                    "        from \"SensorData\" " +
                    "        where \"SensorId\" in ( " +
                    "            select \"Id\" from \"Sensors\" " + 
                    $"            where \"DeviceId\" = '{id}') " +
                    "        group by \"SensorId\" " +
                    "    ) s2 " +
                    " where s1.\"SensorId\" = s2.SensorId " +
                    "    and s1.\"TimeRecord\" = s2.TimeRecord "
                    )
                    .ToListAsync();

            if (dataList.Count == 0) return new object();

            var sensorDataList = dataList.Select(d => new
            {
                Id = d.SensorId,
                Temperature = d.Temperature,
                Humidity = d.Humidity
            }).ToList();

            var sensorCount = sensorDataList.Count;

            double avgTemp = 0;
            double avgHum = 0;

            if (sensorCount > 0)
            {
                avgTemp = sensorDataList.Sum(d => d.Temperature) / sensorCount;
                avgHum = sensorDataList.Sum(d => d.Humidity) / sensorCount;
            }
            
            return new
            {
                Id = id,
                Temperature = avgTemp, 
                Humidity = avgHum,
                Sensors = sensorDataList
            };
        }

        [HttpPost]
        public async Task<ActionResult<Device>> AddDevice(Device device)
        {
            if (device.UserId == Guid.Empty)
                return BadRequest("Device is required to be registered with a user account");
            if (string.IsNullOrEmpty(device.Name))
                return BadRequest("Device name is required");
            
            var alreadyExist = await _context.Devices.AnyAsync(d => d.Id == device.Id);
            if (alreadyExist) return Conflict("Device was already registered");
            
            if (device.Id == Guid.Empty)
                device.Id = Guid.NewGuid();
            
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
            return device;
        }

        [HttpPut]
        public async Task<ActionResult<Device>> UpdateDevice(Device device)
        {
            var found = await _context.Devices.AnyAsync(d => d.Id == device.Id);
            if (!found) return NotFound("Could not find device to update");
            
            _context.Entry(device).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return UnprocessableEntity("DB-ERR: update failed");
            }

            return device;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Device>> DeleteDevice(Guid id)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound("Could not find device to delete");

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return device;
        }
    }
}