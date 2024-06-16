using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using System.IO;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PersonManagement.Controllers
{
    public class PersonController : Controller
    {

        private readonly AppDbContext _appDbContext;
        private readonly int pageSize = 2;

        public PersonController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string identityNumber, int? gender, string name, string taxNumber, int? countryId, int pageNumber = 1)
        {
            //var persons = await _appDbContext.Person.ToListAsync();
            //var persons = await _appDbContext.Person.Skip((pageNumber -1 ) * pageSize).Take(pageSize).ToListAsync();

            // load foreign models items to lists
            List<Country> countries = _appDbContext.Country.ToList();

            // populate foreign models dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", countryId);

            // load object with foreign objects data
            var personsExtended = _appDbContext.Person
            .Join(_appDbContext.Address,
                p => p.Id,
                a => a.PersonId,
                (p, a) => new
                {
                    p.Id,
                    p.IdentityNumber,
                    p.Gender,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    p.Phone,
                    p.Description,
                    a.CountryId
                })
            .Join(_appDbContext.TaxNumber,
                p => p.Id,
                tn => tn.PersonId,
                (p, tn) => new
                {
                    p.Id,
                    p.IdentityNumber,
                    p.Gender,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    p.Phone,
                    p.Description,
                    p.CountryId,
                    TaxNumber = tn.Number
                })
            .ToList();


            // filter list by primary model params and foreign models params
            if (!string.IsNullOrEmpty(identityNumber))
            {
                personsExtended = personsExtended.Where(obj => obj.IdentityNumber.ToUpper() == identityNumber.ToUpper()).ToList();  // exact match
            }
            if (gender != null)
            {
                personsExtended = personsExtended.Where(obj => (int)obj.Gender == gender).ToList();
            }
            if (!string.IsNullOrEmpty(name))
            {
                personsExtended = personsExtended.Where(obj => obj.FirstName.ToUpper().StartsWith(name.ToUpper()) || obj.LastName.ToUpper().StartsWith(name.ToUpper())).ToList(); // like operator
            }
            if (!string.IsNullOrEmpty(taxNumber))
            {
                personsExtended = personsExtended.Where(p => p.TaxNumber.ToUpper().StartsWith(taxNumber.ToUpper())).ToList();
            }
            if (countryId != null)
            {
                personsExtended = personsExtended.Where(p => p.CountryId == countryId).ToList();
            }

            // convert to person object to distinct
            var persons = personsExtended.Select(p => new Person()
            {
                Id = p.Id,
                IdentityNumber = p.IdentityNumber,
                Gender = p.Gender,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Phone = p.Phone,
                Description = p.Description
            }).ToList();

            // .Distinct() doesn't work. workaround...
            persons = persons.GroupBy(p => p.Id).Select(g => g.First()).ToList();

            // store current filter values
            ViewBag.FilterParamIdentityNumber = identityNumber;
            ViewBag.FilterParamGender = gender;
            ViewBag.FilterParamName = name;
            ViewBag.FilterParamTaxNumber = taxNumber;
            ViewBag.FilterParamCountryId = countryId;

            // sort list by primary model params and foreign models params
            ViewBag.SortParamId = sortOrder == "Id" ? "Id_DESC" : "Id";
            ViewBag.SortParamIdentityNumber = sortOrder == "identityNumber" ? "identityNumber_DESC" : "identityNumber";
            ViewBag.SortParamGender = sortOrder == "gender" ? "gender_DESC" : "gender";
            ViewBag.SortParamFirstName = sortOrder == "firstName" ? "firstName_DESC" : "firstName";
            ViewBag.SortParamLastName = sortOrder == "lastName" ? "lastName_DESC" : "lastName";

            switch (sortOrder)
            {
                case "Id":
                    persons = persons.OrderBy(obj => obj.Id).ToList();
                    break;
                case "Id_DESC":
                    persons = persons.OrderByDescending(obj => obj.Id).ToList();
                    break;
                case "identityNumber":
                    persons = persons.OrderBy(obj => obj.IdentityNumber).ToList();
                    break;
                case "identityNumber_DESC":
                    persons = persons.OrderByDescending(obj => obj.IdentityNumber).ToList();
                    break;
                case "gender":
                    persons = persons.OrderBy(obj => obj.Gender).ToList();
                    break;
                case "gender_DESC":
                    persons = persons.OrderByDescending(obj => obj.Gender).ToList();
                    break;
                case "firstName":
                    persons = persons.OrderBy(obj => obj.FirstName).ToList();
                    break;
                case "firstName_DESC":
                    persons = persons.OrderByDescending(obj => obj.FirstName).ToList();
                    break;
                case "lastName":
                    persons = persons.OrderBy(obj => obj.LastName).ToList();
                    break;
                case "lastName_DESC":
                    persons = persons.OrderByDescending(obj => obj.LastName).ToList();
                    break;
                default:
                    persons = persons.OrderBy(obj => obj.Id).ToList();    // on page load
                    break;
            }

            return View(persons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // load foreign models items
            List<Country> countries = _appDbContext.Country.ToList();

            // populate foreign models dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text", null);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person, int[] taxNumberId, string[] taxNumber, string[] taxNumberCountryId, int[] addressId, string[] street, string[] city, string[] zip, string[] addressCountryId)
        {
            ModelState["TaxNumber"].ValidationState = ModelValidationState.Invalid;
            ModelState["Address"].ValidationState = ModelValidationState.Invalid;

            // set taxnumber object list
            List<TaxNumber> TaxNumber = new List<TaxNumber>();
            for (int i = 0; i < taxNumber.Length; i++)
            {
                // skip template and non valid items
                if (string.IsNullOrWhiteSpace(taxNumber[i]) && taxNumberCountryId[i] == null)
                {
                    //  skip validation for template or empty
                    ModelState["TaxNumber"].ValidationState = ModelValidationState.Valid;
                    ModelState["TaxNumber"].Errors.Clear();
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(taxNumber[i]) || taxNumberCountryId[i] == null)
                {
                    // validation for missing values
                    ModelState["TaxNumber"].ValidationState = ModelValidationState.Invalid;
                    ModelState["TaxNumber"].Errors.Add("All fields are required");
                    continue;
                    //return Create();
                }

                TaxNumber obj = new TaxNumber();
                //obj.Id = id[i];
                obj.Number = taxNumber[i];
                obj.CountryId = int.Parse(taxNumberCountryId[i]);
                //obj.PersonId = person.Id;
                TaxNumber.Add(obj);
            }

            // set address object list
            List<Address> Address = new List<Address>();
            for (int i = 0; i < street.Length; i++)
            {
                // skip template and non valid items
                if (string.IsNullOrWhiteSpace(street[i]) && addressCountryId[i] == null)
                {
                    // skip validation for template or empty
                    ModelState["Address"].ValidationState = ModelValidationState.Valid;
                    ModelState["Address"].Errors.Clear();
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(street[i]) ||
                    string.IsNullOrWhiteSpace(city[i]) ||
                    string.IsNullOrWhiteSpace(zip[i]) ||
                    addressCountryId[i] == null)
                {
                    // validation for missing values
                    ModelState["Address"].ValidationState = ModelValidationState.Invalid;
                    ModelState["Address"].Errors.Add("All fields are required");
                    continue;
                    //return Create();
                }

                Address obj = new Address();
                //obj.Id = id[i];
                obj.Street = street[i];
                obj.City = city[i];
                obj.ZIP = zip[i];
                obj.CountryId = int.Parse(addressCountryId[i]);
                //obj.PersonId = person.Id;
                Address.Add(obj);
            }

            // validate object and foreign objects
            if (!ModelState.IsValid)
            {
                return Create();    // to load foreign objects values if they are referenced
            }

            if (person == null)
            {
                return NotFound();
            }

            // store person with all foreign entities
            person.TaxNumber = TaxNumber;
            person.Address = Address;
            _appDbContext.Person.Update(person);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "Person");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            // load object
            var dbPerson = await _appDbContext.Person.FindAsync(id);

            if (dbPerson == null)
            {
                return NotFound();
            }

            // load foreign model objects data for object
            List<TaxNumber> taxnumbers = _appDbContext.TaxNumber.Where(obj => (int)obj.PersonId == id).ToList();
            List<Address> addresses = _appDbContext.Address.Where(obj => (int)obj.PersonId == id).ToList();

            // add foreign objects data to object
            dbPerson.Address = addresses;
            dbPerson.TaxNumber = taxnumbers;

            // load foreign models items
            List<Country> countries = _appDbContext.Country.ToList();

            // populate foreign models dropdown with selected item
            ViewBag.CountryList = new SelectList(countries.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }).ToList(), "Value", "Text");

            return View(dbPerson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Person person, int[] taxNumberId, string[] taxNumber, string[] taxNumberCountryId, int[] addressId, string[] street, string[] city, string[] zip, string[] addressCountryId)
        {
            ModelState["TaxNumber"].ValidationState = ModelValidationState.Invalid;
            ModelState["Address"].ValidationState = ModelValidationState.Invalid;

            // set taxnumber object list
            List<TaxNumber> TaxNumber = new List<TaxNumber>();
            for (int i = 0; i < taxNumber.Length; i++)
            {
                // skip template and non valid items
                if (string.IsNullOrWhiteSpace(taxNumber[i]) && taxNumberCountryId[i] == null)
                {
                    //  skip validation for template or empty
                    ModelState["TaxNumber"].ValidationState = ModelValidationState.Valid;
                    ModelState["TaxNumber"].Errors.Clear();
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(taxNumber[i]) || taxNumberCountryId[i] == null)
                {
                    // validation for missing values
                    ModelState["TaxNumber"].ValidationState = ModelValidationState.Invalid;
                    ModelState["TaxNumber"].Errors.Add("All fields are required");
                    continue;
                    //return Create();
                }

                TaxNumber obj = new TaxNumber();
                //obj.Id = id[i];
                obj.Number = taxNumber[i];
                obj.CountryId = int.Parse(taxNumberCountryId[i]);
                //obj.PersonId = person.Id;
                TaxNumber.Add(obj);
            }

            // set address object list
            List<Address> Address = new List<Address>();
            for (int i = 0; i < street.Length; i++)
            {
                // skip template and non valid items
                if (string.IsNullOrWhiteSpace(street[i]) && addressCountryId[i] == null)
                {
                    // skip validation for template or empty
                    ModelState["Address"].ValidationState = ModelValidationState.Valid;
                    ModelState["Address"].Errors.Clear();
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(street[i]) ||
                    string.IsNullOrWhiteSpace(city[i]) ||
                    string.IsNullOrWhiteSpace(zip[i]) ||
                    addressCountryId[i] == null)
                {
                    // validation for missing values
                    ModelState["Address"].ValidationState = ModelValidationState.Invalid;
                    ModelState["Address"].Errors.Add("All fields are required");
                    continue;
                    //return Create();
                }

                Address obj = new Address();
                //obj.Id = id[i];
                obj.Street = street[i];
                obj.City = city[i];
                obj.ZIP = zip[i];
                obj.CountryId = int.Parse(addressCountryId[i]);
                //obj.PersonId = person.Id;
                Address.Add(obj);
            }

            // validate object and foreign objects
            if (!ModelState.IsValid)
            {
                return await Update(person.Id);    // to load foreign objects values if they are referenced
            }

            // load object
            var dbPerson = await _appDbContext.Person.FindAsync(person.Id);

            if (dbPerson == null)
            {
                return NotFound();
            }

            // set values to person
            dbPerson.Gender = person.Gender;
            dbPerson.IdentityNumber = person.IdentityNumber;
            dbPerson.FirstName = person.FirstName;
            dbPerson.LastName = person.LastName;
            dbPerson.Email = person.Email;
            dbPerson.Phone = person.Phone;
            dbPerson.Description = person.Description;

            // load foreign objects from database to find removed
            List<TaxNumber> delTaxnumber = _appDbContext.TaxNumber.Where(obj => (int)obj.PersonId == person.Id).ToList();
            List<Address> delAddress = _appDbContext.Address.Where(obj => (int)obj.PersonId == person.Id).ToList();

            // find foreign objects to remove
            foreach (var taxnumber in TaxNumber)
            {
                delTaxnumber.RemoveAll(obj => obj.Id == taxnumber.Id);
            }

            foreach (var address in Address)
            {
                delAddress.RemoveAll(obj => obj.Id == address.Id);
            }

            // commit all in transaction
            //try
            //{
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };

            using (var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                options,
                TransactionScopeAsyncFlowOption.Enabled))
            {

                // delete removed foreign objects from database
                if (delAddress.Count() > 0)
                {
                    _appDbContext.Address.RemoveRange(delAddress);
                }
                if (delTaxnumber.Count() > 0)
                {
                    _appDbContext.TaxNumber.RemoveRange(delTaxnumber);
                }

                // update object and update/add foreign objects
                dbPerson.Address = Address;
                dbPerson.TaxNumber = TaxNumber;

                _appDbContext.Person.Update(dbPerson);

                await _appDbContext.SaveChangesAsync();

                transactionScope.Complete();
            }
            //}
            //catch (Exception ex) { }

            return RedirectToAction("Update", "Person", person.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Person person)
        {
            var dbPerson = await _appDbContext.Person
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == person.Id);

            if (dbPerson == null)
            {
                return NotFound();
            }

            _appDbContext.Person.Remove(person);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "Person");
        }

    }
}
