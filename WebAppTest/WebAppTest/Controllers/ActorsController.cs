using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using mwfickling_Assinmgnet3.wwwroot.API.Models;
using mwfickling_MVC_Assingment3.Models;
using System.Numerics;
using System.Text.Json;
using System.Web;
using VaderSharp2;

namespace WebAppTest.Controllers
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
              return _context.Actor != null ? 
                          View(await _context.Actor.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Actor'  is null.");
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Actor == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var actorsVM = new ActorMovieVM();
            actorsVM.Actor = actor;
            actorsVM.ActorId = actor.Id;

            actorsVM.Movies = await _context.MovieActors
                .Where(ma => ma.ActorId == id)     
                .Include(ma => ma.Movie)                
                .Select(ma => ma.Movie)               
                .ToListAsync();

            var analyzer = new SentimentIntensityAnalyzer();
            double sumSentiments = 0;
            int notZeroes = 0;

            try { 
                var posts = await SearchRedditAsync(actor.Name);
                List<RedditPost> sentiments = new();

                foreach(var post in posts)
                {
                    var result = analyzer.PolarityScores(post);
                    var sentiment = result.Compound;

                    if(sentiment!= 0.0)
                    {
                        notZeroes += 1;
                        sumSentiments += result.Compound;
                        sentiments.Add(new RedditPost
                        {
                            Content = post,
                            Sentiment = Math.Round(result.Compound, 2).ToString()
                        });
                    }   
                }

                actorsVM.Sentiment = Math.Round(sumSentiments / notZeroes, 2);
                actorsVM.RedditPosts = sentiments;
            }
            catch
            {
                actorsVM.RedditPosts = new List<RedditPost>();
                actorsVM.RedditPosts.Add(new RedditPost
                {
                    Content = "",
                    Sentiment = ""
                });
                actorsVM.Sentiment = 0.0;
            }


            return View(actorsVM);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile Photo)
        {
            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Photo.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
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
            if (id == null || _context.Actor == null)
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile Photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(Photo != null && Photo.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await Photo.CopyToAsync(memoryStream);
                            actor.Photo = memoryStream.ToArray();
                        }
                    }

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
            if (id == null || _context.Actor == null)
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
            if (_context.Actor == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Actor'  is null.");
            }
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
          return (_context.Actor?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> GetActorPhoto(int id)
        {
            var actor = await _context.Actor.FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var imageData = actor.Photo;

            if (imageData != null)
            {
                return File(imageData, "image/jpg");
            }

            return NotFound();
        }

        public async Task<IActionResult> AddRole(int id)
        {
            if (id == null || _context.Actor == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var actorsVM = new ActorMovieVM();
            actorsVM.Actor = actor;
            actorsVM.ActorId = id;

            actorsVM.MovieItems = await _context.Movie
                .Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToListAsync();

            return View(actorsVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(int ActorId, string MovieId)
        {
            if (string.IsNullOrEmpty(MovieId) || ActorId == 0)
            {
                return NotFound();
            }

            int movieIdInt;

            try
            {
                movieIdInt = int.Parse(MovieId);
            }
            catch
            {
                return NotFound();
            }

            var actorMovie = new MovieActor()
            {
                ActorId = ActorId,
                MovieId = movieIdInt
            };

            _context.Add(actorMovie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteRole(int ActorId, int MovieId)
        {
            if(ActorId == 0 || MovieId == 0) 
            {
                return NotFound();
            }

            var actorMovie = new MovieActor()
            {
                ActorId = ActorId,
                MovieId = MovieId
            };

            _context.Remove(actorMovie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                //fake like you are a "real" web browser
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
                        else if (data.TryGetProperty("title", out JsonElement title)) //use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { returnList.Add(titleValue); }
                        }
                    }
                }
            }
            return returnList;
        }
    }
}
