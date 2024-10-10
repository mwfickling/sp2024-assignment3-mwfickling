using Microsoft.AspNetCore.Mvc.Rendering;
using mwfickling_Assinmgnet3.wwwroot.API.Models;

namespace mwfickling_MVC_Assingment3.Models
{
    public class ActorMovieVM //MANY TO MANY
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int ActorId { get; set;  }
        public Actor Actor { get; set;  }
        public List<Actor> Actors { get; set; } //Actors in the movie
        public List<Movie> Movies { get; set; } //Movies in actor
        public List<SelectListItem> MovieItems { get; set; }
        public List<RedditPost> RedditPosts { get; set; }
        public double Sentiment { get; set; }
    }
}
