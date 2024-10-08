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

namespace Assignment3.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                string after = null; // Pagination token
                int remainingResults = 100;

                while (remainingResults > 0)
                {
                    string url = $"https://www.reddit.com/search.json?limit={Math.Min(remainingResults, 100)}&q={HttpUtility.UrlEncode(searchQuery)}";
                    if (after != null)
                    {
                        url += $"&after={after}";
                    }

                    json = await client.GetStringAsync(url);

                    JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement dataElement = doc.RootElement.GetProperty("data");

                    // Get the "after" token for the next set of results
                    if (dataElement.TryGetProperty("after", out JsonElement afterElement))
                    {
                        after = afterElement.GetString();
                    }

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

                    remainingResults -= 100;

                    // Break if there are no more results
                    if (string.IsNullOrEmpty(after))
                    {
                        break;
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

            var actor = await _context.Actor.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            // Search Reddit for posts related to the movie title
            var redditPosts = await SearchRedditAsync(actor.Name);

            // Perform sentiment analysis on the Reddit posts using VaderSharp2
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

            // Create the ViewModel instance and pass it to the view
            var viewModel = new ActorDetails
            {
                Actor = actor,
                SentimentResults = sentimentResults,
                OverallSentiment = averageSentiment,
                SentimentString = sentimentString
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

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> GetActorPhoto(int id)
        {
            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }
            var imgData = actor.ActorPhoto;

            return File(imgData, "image/jpg");
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMBDHyperlink,ActorPhoto")] Actor actor, IFormFile ActorPhoto)
        {
            ModelState.Remove(nameof(actor.ActorPhoto));

            if (ModelState.IsValid)
            {
                if (ActorPhoto != null)
                {
                    var memoryStream = new MemoryStream();
                    await ActorPhoto.CopyToAsync(memoryStream);
                    actor.ActorPhoto = memoryStream.ToArray();
                } 
                else
                {
                    actor.ActorPhoto = new byte[0];
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMBDHyperlink")] Actor actor, IFormFile ActorPhoto)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ActorPhoto != null && ActorPhoto.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await ActorPhoto.CopyToAsync(memoryStream);
                            actor.ActorPhoto = memoryStream.ToArray(); // Update the MovieCover if a new file is uploaded
                        }
                    }
                    // If no new cover is uploaded, keep the existing one in the database
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }


        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
