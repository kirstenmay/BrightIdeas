using System.ComponentModel.DataAnnotations;

namespace BrightIdeas.Models
{
    public class Like
    {
        [Key]
        public int LikeId {get;set;}
        public int UserId {get;set;}
        public int IdeaId {get;set;}
        public User Alias {get;set;}
        public Idea Post {get;set;}
    }
}