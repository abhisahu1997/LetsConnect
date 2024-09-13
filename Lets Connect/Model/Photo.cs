using System.ComponentModel.DataAnnotations.Schema;

namespace Lets_Connect.Model
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }

        //Navigation Properties
        public int UserID { get; set; }
        public User User { get; set; } = null!;
    }
}