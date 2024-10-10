namespace mwfickling_Assinmgnet3.wwwroot.API.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? IMDBLink { get; set; }
        public string? Genre { get; set; }
        public string? ReleaseYear { get; set; }
        public byte[]? Poster { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }

    }

}
