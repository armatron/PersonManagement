using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PersonManagement.Controllers
{
    public class TaxNumberController : Controller
    {
       
        private readonly AppDbContext _appDbContext;
        private readonly int pageSize = 2;

        public TaxNumberController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string taxNumber, int? countryId, int? personId, int pageNumber = 1)
        {
            // load primary model items to list
            var taxnumbers = await _appDbContext.TaxNumber.ToListAsync();
            //var taxnumbers = await _appDbContext.TaxNumber.Skip((pageNumber -1 ) * pageSize).Take(pageSize).ToListAsync();

            // load foreign models items to lists
            List<Country> countries = _appDbContext.Country.ToList();
            List<Person> persons = _appDbContext.Person.ToList();

            // apply foreign model data for each item in list
            foreach (var taxnumber in taxnumbers)
            {
                taxnumber.Country = countries.FirstOrDefault(obj => obj.Id == taxnumber.CountryId);
            }
            foreach (var taxnumber in taxnumbers)
            {
                taxnumber.Person = persons.FirstOrDefault(obj => obj.Id == taxnumber.PersonId);
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
            if (!string.IsNullOrEmpty(taxNumber))
            {
                taxnumbers = taxnumbers.Where(obj => obj.Number.ToUpper().StartsWith(taxNumber.ToUpper())).ToList();  // like operator
            }
            if (countryId != null)
            {
                taxnumbers = taxnumbers.Where(obj => obj.Country.Id == countryId).ToList();
            }
            if (personId != null)
            {
                taxnumbers = taxnumbers.Where(obj => obj.Person.Id == personId).ToList();
            }
            ViewBag.FilterParamNumber = taxNumber;
            ViewBag.FilterParamCountryId = countryId;
            ViewBag.FilterParamPersonId = personId;

            // sort list by primary model params and foreign models params
            ViewBag.SortParamId = sortOrder == "Id" ? "Id_DESC" : "Id";
            ViewBag.SortParamNumber = sortOrder == "taxNumber" ? "taxNumber_DESC" : "taxNumber";
            ViewBag.SortParamCountry = sortOrder == "countryName" ? "countryName_DESC" : "countryName";
            ViewBag.SortParamPerson = sortOrder == "personFirstName" ? "personFirstName_DESC" : "personFirstName";

            switch (sortOrder)
            {
                case "Id":
                    taxnumbers = taxnumbers.OrderBy(obj => obj.Id).ToList();
                    break;
                case "Id_DESC":
                    taxnumbers = taxnumbers.OrderByDescending(obj => obj.Id).ToList();
                    break;
                case "taxNumber":
                    taxnumbers = taxnumbers.OrderBy(obj => obj.Number).ToList();
                    break;
                case "taxNumber_DESC":
                    taxnumbers = taxnumbers.OrderByDescending(obj => obj.Number).ToList();
                    break;
                case "countryName":
                    taxnumbers = taxnumbers.OrderBy(obj => obj.Country.Name).ToList();
                    break;
                case "countryName_DESC":
                    taxnumbers = taxnumbers.OrderByDescending(obj => obj.Country.Name).ToList();
                    break;
                case "personFirstName":
                    taxnumbers = taxnumbers.OrderBy(obj => obj.Person.FirstName).ToList();
                    break;
                case "personFirstName_DESC":
                    taxnumbers = taxnumbers.OrderByDescending(obj => obj.Person.FirstName).ToList();
                    break;
                default:
                    taxnumbers = taxnumbers.OrderBy(obj => obj.Id).ToList();    // on page load
                    break;
            }

            return View(taxnumbers);
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
        public async Task<IActionResult> Create(TaxNumber taxNumber)
        {
            /*
            if (!ModelState.IsValid)
            {
                return View();
            }
            */
            if (taxNumber is not null)
            {
                _appDbContext.TaxNumber.Update(taxNumber);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "TaxNumber");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taxNumber = await _appDbContext.TaxNumber.FindAsync(id);

            if (taxNumber == null)
            {
                return NotFound();
            }

            PersonController persons = new PersonController(_appDbContext);
            ViewBag.PersonList = persons.PersonDropDown();

            CountryController countries = new CountryController(_appDbContext);
            ViewBag.CountryList = countries.CountryDropDown();

            return View(taxNumber);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TaxNumber taxNumber)
        {
            /*
            if (!ModelState.IsValid)
            {
                return View("TaxNumber");   // return View() does not populate drodown
            }
            */
            var dbTaxNumber = await _appDbContext.TaxNumber.FindAsync(taxNumber.Id);

            if (dbTaxNumber is not null)
            {
                dbTaxNumber.Number = taxNumber.Number;
                dbTaxNumber.CountryId = taxNumber.CountryId;
                dbTaxNumber.PersonId = taxNumber.PersonId;

                _appDbContext.TaxNumber.Update(dbTaxNumber);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "TaxNumber");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(TaxNumber taxNumber)
        {
            if (taxNumber == null)
            {
                return NotFound();
            }

            var dbTaxNumber = await _appDbContext.TaxNumber
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == taxNumber.Id);

            if (dbTaxNumber is not null)
            {
                _appDbContext.TaxNumber.Remove(taxNumber);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "TaxNumber");
        }


        
    }
}
