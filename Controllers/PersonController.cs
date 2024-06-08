using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;

namespace PersonManagement.Controllers
{
    public class PersonController : Controller
    {

        private readonly AppDbContext _appDbContext;

        public PersonController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var persons = await _appDbContext.Person.ToListAsync();

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

            await _appDbContext.SaveChangesAsync();

            return View(person);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Person person)
        {

            var dbPerson = await _appDbContext.Person.FindAsync(person.Id);

            if (dbPerson is not null)
            {
                dbPerson.IdentityNumber = person.IdentityNumber;
                dbPerson.FirstName = person.FirstName;
                dbPerson.LastName = person.LastName;
                dbPerson.Email = person.Email;
                dbPerson.Phone = person.Phone;

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
    }
}
