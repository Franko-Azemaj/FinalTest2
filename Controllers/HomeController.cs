using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FinalTest2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FinalTest2.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger,MyContext context)
    {
        _logger = logger;
        _context = context;

    }

    public IActionResult Index()
    {
        
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        List<Request> rq = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();

var allUser = _context.Users
                .Include(e => e.ReciverRequests)
                .Include(e => e.SenderRequests)
                .ToList();

        List<User> LIST4 = _context.Users
                        .Include(e => e.ReciverRequests)
                        .Include(e => e.SenderRequests)
                        .Where(e => e.UserId != id)
                        .Where(e => 
                                     (e.SenderRequests.Any(f => f.ReciverId == id) == false) 
                                    && (e.ReciverRequests.Any(f => f.SenderId == id) == false)
                        ).ToList();


        List<Request> miqte = _context.Requests.Where(e => (e.SenderId == id) || (e.ReciverId == id)).Include(e => e.Reciver).Include(e => e.Sender).Where(e => e.Accepted == true).ToList();

        ViewBag.perdoruesit = LIST4;
      
    
        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);

        return View();
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {

        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return View();
        }

        return RedirectToAction("Index");

    }
    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
        // Check initial ModelState
        if (ModelState.IsValid)
        {
            // If a User exists with provided email
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                // Manually add a ModelState error to the Email field, with provided
                // error message
                ModelState.AddModelError("UserName", "UserName already in use!");

                return View();
                // You may consider returning to the View at this point
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("userId", user.UserId);

            return RedirectToAction("Index");
        }
        return View();
    }

    [HttpPost("Login")]
    public IActionResult LoginSubmit(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.UserName == userSubmission.UserName);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("Register");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("Register");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId);

            return RedirectToAction("Index");
        }

        return View("Register");
    }
    [HttpGet("SendR/{id}")]
    public IActionResult SendR(int id)
    {
        int idFromSession = (int)HttpContext.Session.GetInt32("userId");
        Request newRequest = new Models.Request()
        {
            SenderId = idFromSession,
            ReciverId = id,

        };
        _context.Requests.Add(newRequest);
        _context.SaveChanges();
        // User dbUser = _context.Users.Include(e=>e.Requests).First(e=> e.UserId == idFromSession);
        // dbUser.Requests.Add(newRequest);
        _context.SaveChanges();
        return RedirectToAction("index");

    }

    [HttpGet("AcceptR/{id}")]
    public IActionResult AcceptR(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        requestii.Accepted = true;
        // _context.Remove(hiqFans);
        _context.SaveChanges();
        return RedirectToAction("myProfile");
    }
    [HttpGet("DeclineR/{id}")]
    public IActionResult Decline(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requestii);
        _context.SaveChanges();
        return RedirectToAction("myProfile");
    }
    [HttpGet("RemoveF/{id}")]
    public IActionResult RemoveF(int id)
    {

        Request requestii = _context.Requests.First(e => e.RequestId == id);
        _context.Remove(requestii);
        _context.SaveChanges();
        return RedirectToAction("myProfile");
    }

     [HttpGet("professional_profile")]
    public IActionResult myProfile()
    {
        int id = (int)HttpContext.Session.GetInt32("userId");

        ViewBag.requests = _context.Requests.Include(e => e.Reciver).Include(e => e.Sender).Where(e => e.ReciverId == id).Where(e => e.Accepted == false).ToList();
        ViewBag.miqte= _context.Requests.Where(e => (e.SenderId == id) || (e.ReciverId == id)).Include(e => e.Reciver).Include(e => e.Sender).Where(e => e.Accepted == true).ToList();

        ViewBag.LogedInUser = _context.Users.First(e => e.UserId == id);

        return View("ProfesionalProfile");
    }

    [HttpGet("users/{id}")]
    public IActionResult users(int id)
    {

        ViewBag.CurrentUser = _context.Users.First(e => e.UserId == id);
        return View("Users");

    }


    [HttpGet("logout")]
    public IActionResult Logout()
    {

        HttpContext.Session.Clear();
        return RedirectToAction("register");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
