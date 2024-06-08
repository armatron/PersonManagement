﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Data;
using PersonManagement.Models;
using System;
using System.Diagnostics.Metrics;

namespace PersonManagement.Controllers
{
    public class CountryController : Controller
    {

        private readonly AppDbContext _appDbContext;

        public CountryController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var countries = await _appDbContext.Country.ToListAsync();

            return View(countries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Country country)
        {
            if (country is not null)
            {
                _appDbContext.Country.Update(country);
                await _appDbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Country");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {

            var country = await _appDbContext.Country.FindAsync(id);

            await _appDbContext.SaveChangesAsync();

            return View(country);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Country country)
        {

            var dbCountry = await _appDbContext.Country.FindAsync(country.Id);

            if (dbCountry is not null)
            {
                dbCountry.Name = country.Name;
                
                _appDbContext.Country.Update(dbCountry);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Country");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Country country)
        {
            var dbCountry = await _appDbContext.Country
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == country.Id);
            if (dbCountry is not null)
            {
                _appDbContext.Country.Remove(country);
                await _appDbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Country");
        }
    }
}
