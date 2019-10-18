using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrightIdeas.Models
{
    public class Idea
    {
        [Key]
        public int IdeaId {get;set;}
        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Text {get;set;}
        public DateTime Created_at {get;set;} = DateTime.Now;
        public DateTime Updated_at {get;set;} = DateTime.Now;
        public int UserId {get;set;}
        public User Creator {get;set;}
        public List<Like> IdeaLikes {get;set;}
    }
}