using System;
using System.Collections.Generic;
using System.Linq;
using Bit.OData.ODataControllers;

namespace BitAspNetCoreAngularSSR.Controllers
{
    public class WeatherForecastController : DtoController<WeatherForecastDto>
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [Get]
        public IEnumerable<WeatherForecastDto> Get()
        {
            Random rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
