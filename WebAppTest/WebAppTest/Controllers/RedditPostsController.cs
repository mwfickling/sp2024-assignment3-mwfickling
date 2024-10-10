using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using mwfickling_MVC_Assingment3.Models;
using System.Text.Json;
using System.Web;

namespace WebAppTest.Controllers
{
    public class RedditPostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedditPostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RedditPosts
        public async Task<IActionResult> Index()
        {
              return _context.RedditPost != null ? 
                          View(await _context.RedditPost.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.RedditPost'  is null.");
        }


        // GET: RedditPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RedditPost == null)
            {
                return NotFound();
            }

            var redditPost = await _context.RedditPost
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditPost == null)
            {
                return NotFound();
            }

            return View(redditPost);
        }
        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                //fake like you are a "real" web browser
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = await client.GetStringAsync("https://www.reddit.com/search.json?limit=100&q="+HttpUtility.UrlEncode(searchQuery));
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

        // GET: RedditPosts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RedditPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Sentiment")] RedditPost redditPost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redditPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditPost);
        }

        // GET: RedditPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RedditPost == null)
            {
                return NotFound();
            }

            var redditPost = await _context.RedditPost.FindAsync(id);
            if (redditPost == null)
            {
                return NotFound();
            }
            return View(redditPost);
        }

        // POST: RedditPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Sentiment")] RedditPost redditPost)
        {
            if (id != redditPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redditPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedditPostExists(redditPost.Id))
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
            return View(redditPost);
        }

        // GET: RedditPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RedditPost == null)
            {
                return NotFound();
            }

            var redditPost = await _context.RedditPost
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditPost == null)
            {
                return NotFound();
            }

            return View(redditPost);
        }

        // POST: RedditPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RedditPost == null)
            {
                return Problem("Entity set 'ApplicationDbContext.RedditPost'  is null.");
            }
            var redditPost = await _context.RedditPost.FindAsync(id);
            if (redditPost != null)
            {
                _context.RedditPost.Remove(redditPost);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditPostExists(int id)
        {
          return (_context.RedditPost?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
