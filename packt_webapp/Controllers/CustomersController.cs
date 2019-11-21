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
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomersController:Controller
    {
        ICustomerRepository _customerRepository;
        ILogger<CustomersController> _logger;
        public CustomersController(ICustomerRepository customerRepository, ILogger<CustomersController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _logger.LogInformation("CustomersController started.");
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


        [HttpGet]
        [Route("{id}", Name = "GetSingleCustomer")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(typeof(Customer), 500)]
        public IActionResult GetSingleCustomer(Guid id)
        {
            Customer singleCustomer = _customerRepository.GetSingle(id);
            if (singleCustomer == null)
            {
                //return NotFound();
                throw new Exception($" ----> GetSingleCustomer() NotFound {id}");
            }

            return Ok(AutoMapper.Mapper.Map<CustomerDto>(singleCustomer));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerDto), 201)]
        [ProducesResponseType(typeof(CustomerDto), 400)]
        [ProducesResponseType(typeof(CustomerDto), 500)]
        public IActionResult AddCustomer([FromBody] CustomerDto customerDtoToSave)
        {

            if (customerDtoToSave == null)
            {
                return BadRequest("customerDtoToSave is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Customer customerToSave = AutoMapper.Mapper.Map<Customer>(customerDtoToSave);

            _customerRepository.Add(customerToSave);

            bool result = _customerRepository.Save();

            if (!result)
            {
                //return new StatusCodeResult(500);
                throw new Exception($" ----> AddCustomer() {customerDtoToSave.ToString()}");
            }

            //return Ok(AutoMapper.Mapper.Map<CustomerDto>(customerToSave));
            return CreatedAtRoute("GetSingleCustomer", new {id = customerToSave.Id}, 
                AutoMapper.Mapper.Map<CustomerDto>(customerToSave));

        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(typeof(CustomerUpdateDto), 200)]
        [ProducesResponseType(typeof(CustomerUpdateDto), 400)]
        [ProducesResponseType(typeof(CustomerUpdateDto), 500)]
        public IActionResult UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto customerDtoToSave)
        {

            if (customerDtoToSave == null)
            {
                return BadRequest("customerDtoToSave is null");
            }

            var existingCustomer = _customerRepository.GetSingle(id);

            if (existingCustomer==null)
            {
                //return NotFound();
                throw new Exception($" ----> UpdateCustomer() NotFound {id}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AutoMapper.Mapper.Map(customerDtoToSave, existingCustomer);

            _customerRepository.Update(existingCustomer);

            bool result = _customerRepository.Save();

            if (!result)
            {
                //return new StatusCodeResult(500);
                throw new Exception($" ----> UpdateCustomer() Error {id}");
            }

            return Ok(AutoMapper.Mapper.Map<CustomerUpdateDto>(existingCustomer));

        }

        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(CustomerUpdateDto), 200)]
        [ProducesResponseType(typeof(CustomerUpdateDto), 400)]
        [ProducesResponseType(typeof(CustomerUpdateDto), 500)]
        public IActionResult PartiallyUpdate(Guid id, [FromBody] JsonPatchDocument<CustomerUpdateDto> CustomerPatchDoc)
        {

            if (CustomerPatchDoc == null)
            {
                //return BadRequest();
                throw new Exception($" ----> PartiallyUpdate() BadRequest {id}");
            }

            var existingCustomer = _customerRepository.GetSingle(id);

            if (existingCustomer == null)
            {
                //return NotFound();
                throw new Exception($" ----> PartiallyUpdate() NotFound {id}");
            }

            var customerToPatch = AutoMapper.Mapper.Map<CustomerUpdateDto>(existingCustomer);
            CustomerPatchDoc.ApplyTo(customerToPatch, ModelState);

            TryValidateModel(customerToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AutoMapper.Mapper.Map(customerToPatch, existingCustomer);

            _customerRepository.Update(existingCustomer);

            bool result = _customerRepository.Save();

            if (!result)
            {
                //return new StatusCodeResult(500);
                throw new Exception($" ----> PartiallyUpdate() Error {id}");
            }

            return Ok(AutoMapper.Mapper.Map<CustomerUpdateDto>(existingCustomer));

        }


        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(NoContentResult), 500)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public IActionResult DeleteCustomer(Guid id)
        {

            Customer customerToDelete = _customerRepository.GetSingle(id);

            if (customerToDelete == null)
            {
                //return NotFound();
                throw new Exception($" ----> DeleteCustomer() NotFound {id}");
            }

            _customerRepository.Delete(id);

            bool result = _customerRepository.Save();

            if (!result)
            {
                //return BadRequest();
                throw new Exception($" ----> DeleteCustomer() BadRequest {id}");
            }

            return NoContent();

        }
    }
}
