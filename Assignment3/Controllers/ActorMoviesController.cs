using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;

namespace Assignment3.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActorToMovie(int actorId, int movieId)
        {
            try
            {
                if (_context.ActorMovies.Any(am => am.ActorId == actorId && am.MovieId == movieId))
                {
                    return Json(new { success = false, message = "Actor is already added to the movie." });
                }

                var actorMovie = new ActorMovie
                {
                    ActorId = actorId,
                    MovieId = movieId
                };

                _context.ActorMovies.Add(actorMovie);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine(ex); // Or use a logging framework
                return Json(new { success = false, message = "An error occurred while adding the actor." });
            }
        }


        // GET: ActorMovies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ActorMovies.Include(a => a.Actor).Include(a => a.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ActorMovies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // GET: ActorMovies/Create
        public IActionResult Create()
        {
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Gender");
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Genre");
            return View();
        }

        // POST: ActorMovies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActorId,MovieId")] ActorMovie actorMovie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Gender", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Genre", actorMovie.MovieId);
            return View(actorMovie);
        }

        // GET: ActorMovies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie == null)
            {
                return NotFound();
            }
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Gender", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Genre", actorMovie.MovieId);
            return View(actorMovie);
        }

        // POST: ActorMovies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActorId,MovieId")] ActorMovie actorMovie)
        {
            if (id != actorMovie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actorMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorMovieExists(actorMovie.Id))
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
            ViewData["ActorId"] = new SelectList(_context.Actor, "Id", "Gender", actorMovie.ActorId);
            ViewData["MovieId"] = new SelectList(_context.Movie, "Id", "Genre", actorMovie.MovieId);
            return View(actorMovie);
        }

        // GET: ActorMovies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // POST: ActorMovies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorMovies.Remove(actorMovie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorMovieExists(int id)
        {
            return _context.ActorMovies.Any(e => e.Id == id);
        }
    }
}
