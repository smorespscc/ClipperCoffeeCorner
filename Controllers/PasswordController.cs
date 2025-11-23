using System.Collections.Generic;
using System.Linq;
using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    public class PasswordController : Controller
    {
        // Simple in-memory store for demonstration. 
        // In production you would inject AppDbContext instead.
        private static readonly List<Password> _store = new();

        public IActionResult Index() => View(_store);

        public IActionResult Details(int userId)
        {
            var item = _store.FirstOrDefault(p => p.UserId == userId);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Password model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_store.Any(p => p.UserId == model.UserId))
            {
                ModelState.AddModelError(nameof(model.UserId),
                    "A password entry for this user already exists.");
                return View(model);
            }

            _store.Add(model);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int userId)
        {
            var item = _store.FirstOrDefault(p => p.UserId == userId);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Password model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = _store.FirstOrDefault(p => p.UserId == model.UserId);
            if (existing == null) return NotFound();
            existing.PasswordHash = model.PasswordHash;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int userId)
        {
            var item = _store.FirstOrDefault(p => p.UserId == userId);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int userId)
        {
            var item = _store.FirstOrDefault(p => p.UserId == userId);
            if (item != null) _store.Remove(item);
            return RedirectToAction(nameof(Index));
        }
    }
}
