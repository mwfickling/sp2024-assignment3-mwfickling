using System.ComponentModel.DataAnnotations;

namespace mwfickling_Assinmgnet3.wwwroot.API.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name= "Gender")]
        public string? Gender { get; set; } //? indicates the value can be read as a string or null (error handling)
        [Display(Name="Age")]
        public int? Age { get; set; }

        [Display(Name = "IMDB Link")]

        public string? IMDBLink { get; set; }
        public byte[]? Photo { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }

    }

}
