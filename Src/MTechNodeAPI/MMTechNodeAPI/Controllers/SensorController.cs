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
    [Route(template:"api/[controller]")]
    
    //creating a sensorController
    public class SensorController : ControllerBase
    {
        private readonly MMTechDbContext _context;

        public SensorController(MMTechDbContext context)
        {
            _context = context;
        }

        //GET: retrieving data of id value of sensor
        [HttpGet(template:"{id}")]
        public async Task<ActionResult<Sensor>> GetSensor(Guid id)
        {
            var sensor = await _context.Sensors
                .FirstOrDefaultAsync(s => s.Id == id); //Lambda expression: mini function
            if (sensor == null)
            {
                return NotFound();
            }
            return sensor;
        }

        [HttpGet("device/{id}")]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetDeviceSensors(Guid id)
        {
            return await _context.Sensors.Where(s => s.DeviceId == id).ToListAsync();
        }

        [HttpGet("data/{id}")]
        public async Task<ActionResult<object>> GetSensorData(Guid id)
        {
            var dataList = await _context.SensorData
                .Where(d => d.SensorId == id)
                .OrderByDescending(d => d.TimeRecord)
                .Take(1)
                .ToListAsync();

            var data = dataList.FirstOrDefault();
            return data == null ? new object()
                : new {Id = data.SensorId, data.Temperature, data.Humidity};
        }
        
        //POST: inserts new record of the sensor
        [HttpPost]
        public async Task<ActionResult<Sensor>> AddSensor(Sensor sensor)
        {
            if (sensor.DeviceId == Guid.Empty)
                return BadRequest("Sensor is required to be registered with a device");
            if (string.IsNullOrEmpty(sensor.Name))
                return BadRequest("Sensor name is required");
            
            var alreadyExist = await _context.Sensors.AnyAsync(s => s.Id == sensor.Id);
            if (alreadyExist) return Conflict("Sensor was already registered");
            
            if (sensor.Id == Guid.Empty)
                sensor.Id = Guid.NewGuid();
            
            //Async allows for multiple processing of queues
            await _context.Sensors.AddAsync(sensor);
            await _context.SaveChangesAsync();
            return sensor;
        }
        
        //PUT: updates existing ID and throws exceptions
        //when multiple updates crash into each other
        [HttpPut(template:"{Id}")]
        public async Task<ActionResult<Sensor>> UpdateSensor(Sensor sensor)
        {
            var found = await _context.Sensors.AnyAsync(s => s.Id == sensor.Id);
            if (!found) return NotFound("Could not find sensor to update");

            _context.Entry(sensor).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return UnprocessableEntity("DB-ERR: update failed");
            }

            return sensor;
        }
        
        //DELETE: deletes record of sensor ID
        [HttpDelete(template:"{id}")]
        public async Task<ActionResult<Sensor>> DeleteSensor(Guid id)
        {
            var sensor = await _context.Sensors.FirstOrDefaultAsync(s => s.Id == id);
            if (sensor == null)
            {
                return NotFound("Could not find sensor to delete");
            }
            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();

            return sensor;
        }
    }
}