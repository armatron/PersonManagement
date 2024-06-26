﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using System.Diagnostics.Metrics;
using System.Net;

namespace PersonManagement.Controllers
{
    public class CountryController : Controller
    {

        private readonly AppDbContext _appDbContext;
        private readonly int pageSize = 2;

        public CountryController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string name, int? countryId, int? personId, int pageNumber = 1)
        {
            var countries = await _appDbContext.Country.ToListAsync();
            //var countries = await _appDbContext.Country.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // filter list by primary model params and foreign models params
            if (!string.IsNullOrEmpty(name))
            {
                countries = countries.Where(obj => obj.Name.ToUpper().StartsWith(name.ToUpper())).ToList();   // like operator
            }
            ViewBag.FilterParamName = name;

            // sort list by primary model params and foreign models params
            ViewBag.SortParamId = sortOrder == "Id" ? "Id_DESC" : "Id";
            ViewBag.SortParamName = sortOrder == "name" ? "name_DESC" : "name";
          
            switch (sortOrder)
            {
                case "Id":
                    countries = countries.OrderBy(obj => obj.Id).ToList();
                    break;
                case "Id_DESC":
                    countries = countries.OrderByDescending(obj => obj.Id).ToList();
                    break;
                case "name":
                    countries = countries.OrderBy(obj => obj.Name).ToList();
                    break;
                case "name_DESC":
                    countries = countries.OrderByDescending(obj => obj.Name).ToList();
                    break;
                    countries = countries.OrderBy(obj => obj.Id).ToList();    // on page load
                    break;
            }

            return View(countries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            ModelState["TaxNumber"].ValidationState = ModelValidationState.Skipped;
            ModelState["Address"].ValidationState = ModelValidationState.Skipped;
            if (!ModelState.IsValid)
            {
                return Create();    // to load foreign objects values if they are referenced
            }
            
            if (country == null)
            {
                return NotFound();
            }

            _appDbContext.Country.Update(country);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "Country");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            var country = await _appDbContext.Country.FindAsync(id);

            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Country country)
        {
            ModelState["TaxNumber"].ValidationState = ModelValidationState.Skipped;
            ModelState["Address"].ValidationState = ModelValidationState.Skipped;
            if (!ModelState.IsValid)
            {
                return await Update(country.Id);    // to load foreign objects values if they are referenced
            }
            
            var dbCountry = await _appDbContext.Country.FindAsync(country.Id);

            if (dbCountry == null)
            { 
                return NotFound(); 
            }

            dbCountry.Name = country.Name;
                
            _appDbContext.Country.Update(dbCountry);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Update", "Country", country.Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Country country)
        {
            var dbCountry = await _appDbContext.Country
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == country.Id);
            
            if (dbCountry == null)
            {
                return NotFound();
            }

            _appDbContext.Country.Remove(country);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("List", "Country");
        }

    }
}
