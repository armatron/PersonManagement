using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;

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
        public async Task<IActionResult> List(string sortOrder, string identityNumber, int? gender, string firstName, string lastName, int pageNumber = 1)
        {
            var persons = await _appDbContext.Person.ToListAsync();
            //var persons = await _appDbContext.Person.Skip((pageNumber -1 ) * pageSize).Take(pageSize).ToListAsync();

            // filter list by primary model params and foreign models params
            if (!string.IsNullOrEmpty(identityNumber))
            {
                persons = persons.Where(obj => obj.IdentityNumber.ToUpper() == identityNumber.ToUpper()).ToList();  // exact match
            }
            if (gender != null)
            {
                persons = persons.Where(obj => (int)obj.Gender == gender).ToList();
            }
            if (!string.IsNullOrEmpty(firstName))
            {
                persons = persons.Where(obj => obj.FirstName.ToUpper().StartsWith(firstName.ToUpper())).ToList(); // like operator
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                persons = persons.Where(obj => obj.LastName.ToUpper().StartsWith(lastName.ToUpper())).ToList(); // like operator
            }
            ViewBag.FilterParamIdentityNumber = identityNumber;
            ViewBag.FilterParamGender = gender;
            ViewBag.FilterParamFirstName = firstName;
            ViewBag.FilterParamLastName = lastName;

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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Person person)
        {
            /*
            if (!ModelState.IsValid)
            {
                return Create();    // return View() does not populate drodown
            }
            */
            if (person is not null)
            {
                _appDbContext.Person.Update(person);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Person");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            var person = await _appDbContext.Person.FindAsync(id);

            //await _appDbContext.SaveChangesAsync();

            return View(person);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Person person)
        {

            var dbPerson = await _appDbContext.Person.FindAsync(person.Id);

            if (dbPerson is not null)
            {
                dbPerson.Gender = person.Gender;
                dbPerson.IdentityNumber = person.IdentityNumber;
                dbPerson.FirstName = person.FirstName;
                dbPerson.LastName = person.LastName;
                dbPerson.Email = person.Email;
                dbPerson.Phone = person.Phone;
                dbPerson.Description = person.Description;

                _appDbContext.Person.Update(dbPerson);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Person");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Person person)
        {
            var dbPerson = await _appDbContext.Person
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == person.Id);
            if (dbPerson is not null)
            {
                _appDbContext.Person.Remove(person);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Person");
        }

        public IEnumerable<SelectListItem> PersonDropDown()
        {
            IEnumerable<SelectListItem> persons =
                _appDbContext.Person.Select(i => new SelectListItem
                {
                    Text = $"{i.FirstName} {i.LastName}",
                    Value = i.Id.ToString()
                });

            return persons;
        }
        /*
        public IEnumerable<SelectListItem> GenderDropDown()
        {
            yield return new SelectListItem { 
                Text = "Male", 
                Value = Gender.Male.ToString() 
            };
            yield return new SelectListItem { 
                Text = "Female", 
                Value = Gender.Female.ToString() 
            };
        }
        */
    }
}
