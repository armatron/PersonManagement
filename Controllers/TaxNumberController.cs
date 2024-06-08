using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;

namespace PersonManagement.Controllers
{
    public class TaxNumberController : Controller
    {
       
        private readonly AppDbContext _appDbContext;

        public TaxNumberController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var taxnumbers = await _appDbContext.TaxNumber.ToListAsync();

            // load FK objects
            foreach (var taxnumber in taxnumbers) {
                taxnumber.Country = _appDbContext.Country.FirstOrDefault(obj => obj.Id == taxnumber.CountryId);
            }
            foreach (var taxnumber in taxnumbers)
            {
                taxnumber.Person = _appDbContext.Person.FirstOrDefault(obj => obj.Id == taxnumber.PersonId);
            }
            return View(taxnumbers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateCountryList();
            PopulatePersonList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaxNumber taxNumber)
        {
            if (taxNumber is not null)
            {
                _appDbContext.TaxNumber.Update(taxNumber);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "TaxNumber");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            var taxNumber = await _appDbContext.TaxNumber.FindAsync(id);

            await _appDbContext.SaveChangesAsync();

            return View(taxNumber);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TaxNumber taxNumber)
        {

            var dbTaxNumber = await _appDbContext.TaxNumber.FindAsync(taxNumber.Id);

            if (dbTaxNumber is not null)
            {
                dbTaxNumber.Number = taxNumber.Number;
                dbTaxNumber.CountryId = taxNumber.CountryId;
                //dbTaxNumber.PersonId = taxNumber.PersonId;

                _appDbContext.TaxNumber.Update(dbTaxNumber);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "TaxNumber");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(TaxNumber taxNumber)
        {
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

        public void PopulateCountryList()
        {
            IEnumerable<SelectListItem> countries =
                _appDbContext.Country.Select(i => new SelectListItem {
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
