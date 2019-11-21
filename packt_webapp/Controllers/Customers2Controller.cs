using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using packt_webapp.Dtos;
using packt_webapp.Entities;
using packt_webapp.QueryParameters;
using packt_webapp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace packt_webapp.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class Customers2Controller:Controller
    {
        ICustomerRepository _customerRepository;
        ILogger<Customers2Controller> _logger;
        public Customers2Controller(ICustomerRepository customerRepository, ILogger<Customers2Controller> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _logger.LogInformation("Customers2Controller started.");
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Customer>), 200)]
        public IActionResult GetAllCustomers(CustomerQueryParameters customerQueryParameters)
        {
            _logger.LogInformation(" ----> GetAllCustomers()");
            var allCustomers = _customerRepository.GetAll(customerQueryParameters).ToList();
            var allCustomersDto = allCustomers.Select(x => AutoMapper.Mapper.Map<CustomerDto>(x));

            Response.Headers.Add("X-Pagination", 
                Newtonsoft.Json.JsonConvert.SerializeObject(new { totalCount = _customerRepository.Count() })
            );

            return Ok(allCustomersDto);
        }

    }
}
