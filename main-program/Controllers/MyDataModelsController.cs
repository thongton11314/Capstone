/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using program.Data;
using program.Models;

namespace program.Controllers
{
    public class MyDataModelsController : Controller
    {
        private readonly programContext _context;

        public MyDataModelsController(programContext context)
        {
            _context = context;
        }

        // GET: MyDataModels
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.MyDataModel
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.MyDataModel
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var movieGenreVM = new Supermarkets
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };

            return View(movieGenreVM);
        }

        // GET: MyDataModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myDataModel = await _context.MyDataModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myDataModel == null)
            {
                return NotFound();
            }

            return View(myDataModel);
        }

        // GET: MyDataModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyDataModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price")] Supermarkets myDataModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(myDataModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myDataModel);
        }

        // GET: MyDataModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myDataModel = await _context.MyDataModel.FindAsync(id);
            if (myDataModel == null)
            {
                return NotFound();
            }
            return View(myDataModel);
        }

        // POST: MyDataModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Supermarkets myDataModel)
        {
            if (id != myDataModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(myDataModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MyDataModelExists(myDataModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(myDataModel);
        }

        // GET: MyDataModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myDataModel = await _context.MyDataModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myDataModel == null)
            {
                return NotFound();
            }

            return View(myDataModel);
        }

        // POST: MyDataModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myDataModel = await _context.MyDataModel.FindAsync(id);
            _context.MyDataModel.Remove(myDataModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyDataModelExists(int id)
        {
            return _context.MyDataModel.Any(e => e.Id == id);
        }
    }
}
*/