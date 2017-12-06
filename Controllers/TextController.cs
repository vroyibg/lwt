using System;
using LWT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LWT.Controllers
{
    public class TextController:Controller
    {
        private readonly LWTContext _context;

        public TextController(LWTContext context)
        {
            _context = context;
        }
        // The index page show list of all text in current language
        public IActionResult Index()
        {
            var texts = _context.Texts.Include(text => text.Language);
            ListTextViewModel listTextViewModel = new ListTextViewModel()
            {
                Texts = texts.ToList()
            };
            return View(listTextViewModel);
        }
    }
}