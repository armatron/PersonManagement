using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using System.Diagnostics.Metrics;
using System.Net;

namespace PersonManagement.Controllers
{
    public class AddressController : Controller
    {
       
        private readonly AppDbContext _appDbContext;
        private readonly int pageSize = 2;

        public AddressController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string street, string city, string zip, int? countryId, int? personId, int pageNumber = 1)
        {
            var addresses = await _appDbContext.Address.ToListAsync();
            //var addresses = await _appDbContext.Address.Skip((pageNumber -1 ) * pageSize).Take(pageSize).ToListAsync();

            // load foreign models items to lists
            List<Country> countries = _appDbContext.Country.ToList();
            List<Person> persons = _appDbContext.Person.ToList();

            // apply foreign model data for each item in list
            foreach (var address in addresses)
            {
                address.Country = countries.FirstOrDefault(obj => obj.Id == address.CountryId);
            }
            foreach (var address in addresses)
            {
                address.Person = persons.FirstOrDefault(obj => obj.Id == address.PersonId);
            }

            // populate foreign models dropdown
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", countryId);

            ViewBag.PersonList = new SelectList(persons.Select(i => new SelectListItem
            {
                Text = $"{i.FirstName} {i.LastName}",
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", personId);

            // filter list by primary model params and foreign models params
            if (!string.IsNullOrEmpty(street))
            {
                addresses = addresses.Where(obj => obj.Street.ToUpper().StartsWith(street.ToUpper())).ToList();  // like operator
            }
            if (!string.IsNullOrEmpty(city))
            {
                addresses = addresses.Where(obj => obj.City.ToUpper().StartsWith(city.ToUpper())).ToList();  // like operator
            }
            if (!string.IsNullOrEmpty(zip))
            {
                addresses = addresses.Where(obj => obj.ZIP.ToUpper().StartsWith(zip.ToUpper())).ToList();  // like operator
            }
            if (countryId != null)
            {
                addresses = addresses.Where(obj => obj.Country.Id == countryId).ToList();
            }
            if (personId != null)
            {
                addresses = addresses.Where(obj => obj.Person.Id == personId).ToList();
            }
            ViewBag.FilterParamStreet = street;
            ViewBag.FilterParamCity = city;
            ViewBag.FilterParamZip = zip;
            ViewBag.FilterParamCountryId = countryId;
            ViewBag.FilterParamPersonId = personId;

            // sort list by primary model params and foreign models params
            ViewBag.SortParamId = sortOrder == "Id" ? "Id_DESC" : "Id";
            ViewBag.SortParamStreet = sortOrder == "street" ? "street_DESC" : "street";
            ViewBag.SortParamCity = sortOrder == "city" ? "city_DESC" : "city";
            ViewBag.SortParamZip = sortOrder == "zip" ? "zip_DESC" : "zip";
            ViewBag.SortParamCountry = sortOrder == "countryName" ? "countryName_DESC" : "countryName";
            ViewBag.SortParamPerson = sortOrder == "personFirstName" ? "personFirstName_DESC" : "personFirstName";

            switch (sortOrder)
            {
                case "Id":
                    addresses = addresses.OrderBy(obj => obj.Id).ToList();
                    break;
                case "Id_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.Id).ToList();
                    break;
                case "street":
                    addresses = addresses.OrderBy(obj => obj.Street).ToList();
                    break;
                case "street_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.Street).ToList();
                    break;
                case "city":
                    addresses = addresses.OrderBy(obj => obj.City).ToList();
                    break;
                case "city_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.City).ToList();
                    break;
                case "zip":
                    addresses = addresses.OrderBy(obj => obj.ZIP).ToList();
                    break;
                case "zip_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.ZIP).ToList();
                    break;
                case "countryName":
                    addresses = addresses.OrderBy(obj => obj.Country.Name).ToList();
                    break;
                case "countryName_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.Country.Name).ToList();
                    break;
                case "personFirstName":
                    addresses = addresses.OrderBy(obj => obj.Person.FirstName).ToList();
                    break;
                case "personFirstName_DESC":
                    addresses = addresses.OrderByDescending(obj => obj.Person.FirstName).ToList();
                    break;
                default:
                    addresses = addresses.OrderBy(obj => obj.Id).ToList();    // on page load
                    break;
            }

            return View(addresses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PersonController persons = new PersonController(_appDbContext);
            ViewBag.PersonList = persons.PersonDropDown();

            CountryController countries = new CountryController(_appDbContext);
            ViewBag.CountryList = countries.CountryDropDown();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Address address)
        {
            /*
            if (!ModelState.IsValid)
            {
                return Create();    // return View() does not populate drodown
            }
            */
            if (address is not null)
            {
                _appDbContext.Address.Update(address);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Address");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            var address = await _appDbContext.Address.FindAsync(id);

            PersonController persons = new PersonController(_appDbContext);
            ViewBag.PersonList = persons.PersonDropDown();

            CountryController countries = new CountryController(_appDbContext);
            ViewBag.CountryList = countries.CountryDropDown();

            return View(address);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Address address)
        {

            var dbAddress = await _appDbContext.Address.FindAsync(address.Id);

            if (dbAddress is not null)
            {
                dbAddress.Street = address.Street;
                dbAddress.City = address.City;
                dbAddress.ZIP = address.ZIP;
                dbAddress.CountryId = address.CountryId;
                dbAddress.PersonId = address.PersonId;

                _appDbContext.Address.Update(dbAddress);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Address");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Address address)
        {
            var dbAddress = await _appDbContext.Address
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == address.Id);
            if (dbAddress is not null)
            {
                _appDbContext.Address.Remove(address);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Address");
        }
    }
}
