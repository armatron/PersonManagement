using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using System.Net;
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
        public async Task<IActionResult> List(string sortOrder, string taxNumber, int? countryId, string person, int pageNumber = 1)
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

            // populate foreign models dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", countryId);

            /*
            ViewBag.PersonList = new SelectList(persons.Select(i => new SelectListItem
            {
                Text = $"{i.FirstName} {i.LastName}",
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", personId);
            */

            // filter list by primary model params and foreign models params
            if (!string.IsNullOrEmpty(taxNumber))
            {
                taxnumbers = taxnumbers.Where(obj => obj.Number.ToUpper().StartsWith(taxNumber.ToUpper())).ToList();  // like operator
            }
            if (countryId != null)
            {
                taxnumbers = taxnumbers.Where(obj => obj.Country.Id == countryId).ToList();
            }
            if (!string.IsNullOrEmpty(person))
            {
                taxnumbers = taxnumbers.Where(obj => obj.Person.FirstName.ToUpper().StartsWith(person.ToUpper()) || obj.Person.LastName.ToUpper().StartsWith(person.ToUpper())).ToList();
            }
            ViewBag.FilterParamNumber = taxNumber;
            ViewBag.FilterParamCountryId = countryId;
            ViewBag.FilterParamPerson = person;

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
            // load foreign objects items
            List<Country> countries = _appDbContext.Country.ToList();
            List<Person> persons = _appDbContext.Person.ToList();

            // populate foreign objects dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text");

            ViewBag.PersonList = new SelectList(persons.Select(i => new SelectListItem
            {
                Text = $"{i.FirstName} {i.LastName}",
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text");


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaxNumber taxNumber)
        {
            ModelState["Person"].ValidationState = ModelValidationState.Skipped;
            ModelState["Country"].ValidationState = ModelValidationState.Skipped;
            if (!ModelState.IsValid)
            {
                return Create();    // to load foreign objects values if they are referenced
            }
            
            if (taxNumber == null)
            {
                return NotFound();
            }

            _appDbContext.TaxNumber.Update(taxNumber);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "TaxNumber");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            // load object
            var dbTaxNumber = await _appDbContext.TaxNumber.FindAsync(id);

            if (dbTaxNumber == null)
            {
                return NotFound();
            }

            // load foreign objects items
            List<Country> countries = _appDbContext.Country.ToList();
            List<Person> persons = _appDbContext.Person.ToList();

            // populate foreign objects dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text");

            ViewBag.PersonList = new SelectList(persons.Select(i => new SelectListItem
            {
                Text = $"{i.FirstName} {i.LastName}",
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text");

            // person should be read only
            Person person = persons.FirstOrDefault(obj => obj.Id == dbTaxNumber.PersonId);

            ViewBag.PersonName = $"{person.FirstName} {person.LastName}";

            return View(dbTaxNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TaxNumber taxNumber)
        {
            ModelState["Person"].ValidationState = ModelValidationState.Skipped;
            ModelState["Country"].ValidationState = ModelValidationState.Skipped;
            if (!ModelState.IsValid)
            {
                return await Update(taxNumber.Id);    // to load foreign objects values if they are referenced
            }

            var dbTaxNumber = await _appDbContext.TaxNumber.FindAsync(taxNumber.Id);

            if (dbTaxNumber == null)
            {
                return NotFound();
            }

            dbTaxNumber.Number = taxNumber.Number;
            dbTaxNumber.CountryId = taxNumber.CountryId;
            //dbTaxNumber.PersonId = taxNumber.PersonId;    // should not be changed

            _appDbContext.TaxNumber.Update(dbTaxNumber);
            await _appDbContext.SaveChangesAsync();
            
            return RedirectToAction("Update", "TaxNumber", taxNumber.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(TaxNumber taxNumber)
        {
            
            var dbTaxNumber = await _appDbContext.TaxNumber
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == taxNumber.Id);

            if (dbTaxNumber == null)
            {
                return NotFound();
            }

            _appDbContext.TaxNumber.Remove(taxNumber);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "TaxNumber");
        }

    }
}
