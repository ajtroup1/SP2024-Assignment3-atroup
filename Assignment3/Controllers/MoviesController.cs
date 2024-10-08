using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;
using System.Text.Json;
using System.Web;
using VaderSharp2;
using System.Numerics;

namespace Assignment3.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                // fake like you are a "real" web browser
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = await client.GetStringAsync("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(searchQuery));
            }
            var textToExamine = new List<string>();
            JsonDocument doc = JsonDocument.Parse(json);
            // Navigate to the "data" object
            JsonElement dataElement = doc.RootElement.GetProperty("data");
            // Navigate to the "children" array
            JsonElement childrenElement = dataElement.GetProperty("children");
            foreach (JsonElement child in childrenElement.EnumerateArray())
            {
                if (child.TryGetProperty("data", out JsonElement data))
                {
                    if (data.TryGetProperty("selftext", out JsonElement selftext))
                    {
                        string selftextValue = selftext.GetString();
                        if (!string.IsNullOrEmpty(selftextValue)) { returnList.Add(selftextValue); }
                        else if (data.TryGetProperty("title", out JsonElement title)) // use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { returnList.Add(titleValue); }
                        }
                    }
                }
            }
            return returnList;
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var actors = await _context.Actor.ToListAsync();

            var redditPosts = await SearchRedditAsync(movie.Title);

            var analyzer = new SentimentIntensityAnalyzer();
            var sentimentResults = new List<SentimentResult>();
            double overallSentiment = 0;
            int count = 0;

            foreach (var post in redditPosts)
            {
                var result = analyzer.PolarityScores(post);
                double sentimentScore = result.Compound;

                if (sentimentScore != 0)
                {
                    string sentimentStringIndiv = CategorizeSentiment(sentimentScore);
                    sentimentResults.Add(new SentimentResult
                    {
                        Title = "Reddit Post",
                        Content = post,
                        SentimentScore = sentimentScore,
                        SentimentString = sentimentStringIndiv
                    });

                    overallSentiment += sentimentScore;
                    count++;
                }
            }

            double averageSentiment = count > 0 ? overallSentiment / count : 0;
            string sentimentString = CategorizeSentiment(averageSentiment);

            var viewModel = new MovieDetails
            {
                Movie = movie,
                SentimentResults = sentimentResults,
                OverallSentiment = averageSentiment,
                SentimentString = sentimentString,
                Actors = actors
            };

            return View(viewModel);
        }


        private string CategorizeSentiment(double score)
        {
            string sentimentString = "";

            if (score >= -1 && score < -0.6)
            {
                sentimentString = "Extremely Poor";
            } 
            else if (score >= -0.6 && score < -0.2)
            {
                sentimentString = "Poor";
            } 
            else if (score >= -0.2 && score < 0.2)
            {
                sentimentString = "Neutral";
            } 
            else if (score >= 0.2 && score < 0.6)
            {
                sentimentString = "Positive";
            }
            else if (score >= 0.6)
            {
                sentimentString = "Extremely Positive";
            }

            return sentimentString;

        }



        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile MovieCover)
        {
            if (ModelState.IsValid)
            {
                if (MovieCover != null && MovieCover.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await MovieCover.CopyToAsync(memoryStream);
                        movie.MovieCover = memoryStream.ToArray();
                    }
                }
                else
                {
                    movie.MovieCover = new byte[0]; // Or leave as null if preferred
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }


        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie); // Pass the movie to the view
        }


        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,ReleaseDate,ReleasedBy")] Movie movie, IFormFile MovieCover)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (MovieCover != null && MovieCover.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await MovieCover.CopyToAsync(memoryStream);
                            movie.MovieCover = memoryStream.ToArray(); // Update the MovieCover if a new file is uploaded
                        }
                    }
                    // If no new cover is uploaded, keep the existing one in the database
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }


        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }

        public async Task<IActionResult> GetMovieCover(int id)
        {
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var imgData = movie.MovieCover;

            Console.WriteLine("DATA: "+imgData);

            return File(imgData, "image/jpg");
        }

    }
}
