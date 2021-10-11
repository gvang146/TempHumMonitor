using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMTechNodeAPI.Data;
using MMTechNodeAPI.Models;

//We used Postgres DB because its free and were not planning to build a complex database where it would require enterprise DB.
//Every minute of the data getting read, gets pushed up to the web service,
//to be accessed and displayed when called

namespace MMTechNodeAPI.Controllers
{
    [ApiController]
    [Route(template:"api/[controller]")]
    
    public class SensorDataController : ControllerBase
    {
        private readonly MMTechDbContext _context;

        public SensorDataController(MMTechDbContext context)
        {
            _context = context;
        }

        [HttpGet("getfirst/{sensorId}/{num}")]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetFirst(Guid sensorId, int num)
        {
            if (num <= 0)
            {
                num = 1;
            }
            
            var sensorDate = await _context.SensorData
                .Where(d => d.SensorId == sensorId)
                .OrderBy(d => d.TimeRecord)
                .Take(num)
                .ToListAsync();

            return sensorDate;
        }
        
        [HttpGet("getlast/{sensorId}/{num}")]
        public async Task<ActionResult<IEnumerable<SensorData>>> GetLast(Guid sensorId, int num)
        {
            if (num <= 0)
            {
                num = 1;
            }
            var sensorDate = await _context.SensorData
                .Where(d => d.SensorId == sensorId)
                .OrderByDescending(d => d.TimeRecord)
                .Take(num)
                .ToListAsync();
            return sensorDate;
        }

        //POST: insets new record of the sensorData
        [HttpPost]
        public async Task<ActionResult> AddSensorData(SensorData sensorData)
        {
            // client doesn't have to populate client ID
            // less information to transfer
            sensorData.Id = Guid.NewGuid();
            await _context.SensorData.AddAsync(sensorData);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}