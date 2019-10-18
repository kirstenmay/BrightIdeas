using System.Collections.Generic;

namespace BrightIdeas.Models
{
    public class WrapperModel 
    {
        public List<Idea> AllIdeas {get;set;}
        public Idea NewIdea {get;set;}
        public User User {get;set;}
        public int Count {get;set;}
    }
}