using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using solarmhc.Models.Data;
using solarmhc.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace solarmhc.Models.Services
{
    public class PowerDataService
    {
        private readonly IServiceProvider _serviceProvider;

        public PowerDataService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<List<PowerData>> GetPowerDataForDateAsync(string dashboardId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetService<SolarMHCDbContext>();

                var mountainTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                var mountainNow = TimeZoneInfo.ConvertTime(DateTime.Now, mountainTimeZone);

                SolarSegment seg = await _context.SolarSegments.Where(x => x.Name == dashboardId).FirstOrDefaultAsync();
                List<PowerData> pData = await _context.PowerIntakes
                    .Where(x => x.SolarSegmentId == seg.Id && x.TimeStamp.Date.Day == mountainNow.Date.Day)
                    .Select(x => new PowerData
                    {
                        Date = x.TimeStamp,
                        Intake = x.KW
                    })
                    .ToListAsync();

                //TODO: UTC?

                return pData;
            }
        }
    }
}