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

        public AddressController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var addresses = await _appDbContext.Address.ToListAsync();

            // load FK objects
            foreach (var address in addresses)
            {
                address.Country = _appDbContext.Country.FirstOrDefault(obj => obj.Id == address.CountryId);
            }
            foreach (var address in addresses)
            {
                address.Person = _appDbContext.Person.FirstOrDefault(obj => obj.Id == address.PersonId);
            }

            return View(addresses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateCountryList();
            PopulateCountryList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Address address)
        {
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

            await _appDbContext.SaveChangesAsync();

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
                //dbAddress.PersonId = address.PersonId;

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

        public void PopulateCountryList()
        {
            IEnumerable<SelectListItem> countries =
                _appDbContext.Country.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

            ViewBag.CountryList = countries;
        }

        public void PopulatePersonList()
        {
            IEnumerable<SelectListItem> persons =
                _appDbContext.Person.Select(i => new SelectListItem
                {
                    Text = $"{i.FirstName} {i.LastName}",
                    Value = i.Id.ToString()
                });

            ViewBag.PersonList = persons;
        }
    }
}
