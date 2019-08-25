using Bit.Model.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace BitAspNetCoreAngularSSR
{
    public class WeatherForecastDto : IDto
    {
        [Key]
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF
        {
            get => 32 + (int)(TemperatureC / 0.5556);
            set { }
        }

        public string Summary { get; set; }
    }
}
