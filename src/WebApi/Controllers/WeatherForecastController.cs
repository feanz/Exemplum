﻿namespace Exemplum.WebApi.Controllers
{
    using Application.WeatherForecast.Model;
    using Application.WeatherForecast.Query;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class WeatherForecastController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public WeatherForecastController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get weather forecast for a given location
        /// </summary>
        /// /// <remarks>
        /// Sample request:
        ///     GET /weatherforecast?lat=11.96&amp;lon=108.4 
        /// </remarks>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <returns>A weather forecast</returns>
        [HttpGet("weatherforecast")]
        public async Task<ActionResult<WeatherForecast>> Get(decimal lat, decimal lon)
        {
            var forecast = await _mediator.Send(new GetWeatherForecastQuery { Lat = lat, Lon = lon });

            return forecast;
        }
    }
}