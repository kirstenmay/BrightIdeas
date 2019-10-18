using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BrightIdeas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrightIdeas.Controllers
{

    public class HomeController : Controller
    {
         private MyContext dbContext;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ LOGIN REGISTRATION ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Create")]
        public IActionResult Create(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("userId", user.UserId);
                HttpContext.Session.SetString("userName", user.Alias);
                return RedirectToAction("Ideas");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.LoginEmail);
                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.LoginPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Username/password combination incorrect");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                HttpContext.Session.SetString("userName", userInDb.Alias);
                return RedirectToAction("Ideas");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("LogOut")]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }



//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ IDEAS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HttpGet("Ideas")]
        public IActionResult Ideas()
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                WrapperModel viewMod = new WrapperModel();
                viewMod.User = dbContext.Users.FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("userId"));
                viewMod.AllIdeas = dbContext.Ideas.Include(u => u.Creator).Include(c => c.IdeaLikes).OrderByDescending(c => c.IdeaLikes.Count).ToList();
                return View(viewMod);
            }
        }

        [HttpPost("CreateIdea")]
        public IActionResult CreateIdea(WrapperModel idea)
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if(ModelState.IsValid)
                {
                    int? uId = HttpContext.Session.GetInt32("userId");
                    idea.NewIdea.Creator = dbContext.Users.FirstOrDefault(u => u.UserId == (int)uId);
                    dbContext.Add(idea.NewIdea);
                    dbContext.SaveChanges();
                    return RedirectToAction("Ideas");
                }
                else
                {
                     WrapperModel viewMod = new WrapperModel();
                    viewMod.User = dbContext.Users.FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("userId"));
                    viewMod.AllIdeas = dbContext.Ideas.Include(u => u.Creator).Include(c => c.IdeaLikes).OrderByDescending(c => c.IdeaLikes.Count).ToList();
                    return View("Ideas", viewMod);
                }
            }
        }

        [HttpGet("Like/{id}")]
        public IActionResult Like(int id)
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                int? UserId = HttpContext.Session.GetInt32("userId");
                Idea thisIdea = dbContext.Ideas
                    .Include(u => u.IdeaLikes)
                    .ThenInclude(u => u.Alias)
                    .FirstOrDefault(w => w.IdeaId == id);
                User CurrentUser = dbContext.Users.FirstOrDefault(i => i.UserId == UserId);
                var WantingToLike = dbContext.Likes.FirstOrDefault(w => w.IdeaId == id && w.UserId == CurrentUser.UserId);
                if(WantingToLike == null)
                {
                    Like like = new Like 
                    {
                        UserId = (int)UserId,
                        IdeaId = id
                    };
                    dbContext.Add(like);
                    dbContext.SaveChanges();
                }
                else
                {
                    return RedirectToAction("Ideas");
                }
                return RedirectToAction("Ideas");
            }
        }

        [HttpGet("DisplayIdea/{id}")]
        public IActionResult DisplayIdea(int id)
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                WrapperModel viewMod = new WrapperModel();
                viewMod.NewIdea = dbContext.Ideas.Include(i => i.Creator).Include(i => i.IdeaLikes).ThenInclude(u => u.Alias).FirstOrDefault(i => i.IdeaId == id);
                return View(viewMod);
            }
        }

        [HttpGet("DisplayUser/{uId}")]
        public IActionResult DisplayUser(int uId)
        {
            if(HttpContext.Session.GetString("userName") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                WrapperModel viewMod = new WrapperModel();
                viewMod.User = dbContext.Users.Include(u => u.UserLikes).FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("userId"));
                viewMod.AllIdeas = dbContext.Ideas.Include(u => u.Creator).Include(i => i.IdeaLikes).Where(i => i.Creator.UserId == (int)HttpContext.Session.GetInt32("userId")).ToList();
                int sum = 0;
                foreach(Idea idea in viewMod.AllIdeas)
                {
                    sum += idea.IdeaLikes.Count;
                }
                viewMod.Count = sum;
                return View(viewMod);
            }
        }

        [HttpGet("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            Idea thisIdea = dbContext.Ideas
                .Include(u => u.Creator)
                .FirstOrDefault(w => w.IdeaId == id);
            User thisUser = dbContext.Users.FirstOrDefault(u => u.UserId == thisIdea.Creator.UserId);
            int? userId = HttpContext.Session.GetInt32("userId");
            if((int)userId != thisUser.UserId)
            {
                return RedirectToAction("Ideas");
            }
            else
            {
                dbContext.Ideas.Remove(thisIdea);
                dbContext.SaveChanges();
                return RedirectToAction("Ideas");
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
