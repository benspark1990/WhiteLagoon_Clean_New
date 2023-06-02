﻿using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Data;
using WhiteLagoon.Models;

namespace WhiteLagoon.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VillaController(ApplicationDbContext context)
        {
                _context = context;
        }
        public IActionResult Index()
        {
            List<Villa> villaList = _context.Villas.ToList();
            return View(villaList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            
                _context.Villas.Add(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            
        }
    }
}
