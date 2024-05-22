using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StellarIO.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDbContext context,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        _logger.LogInformation("Register GET method called.");
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        _logger.LogInformation("Register POST method called.");
        returnUrl ??= Url.Content("~/");
        if (ModelState.IsValid)
        {
            _logger.LogInformation("ModelState is valid.");
            var user = new User { UserName = Input.Email, Email = Input.Email };
            _logger.LogInformation("Creating user {Email}.", Input.Email);
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully.", Input.Email);

                try
                {
                    _logger.LogInformation("Creating a new planet for user {UserId}.", user.Id);
                    var planet = CreateNewPlanetForUser(user);
                    _context.Planets.Add(planet);

                    _logger.LogInformation("Attempting to save new planet to the database.");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("New planet {PlanetId} created and assigned to user {UserId}.", planet.Id, user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating a new planet for user {UserId}.", user.Id);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                _logger.LogError("Error creating user: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        else
        {
            _logger.LogWarning("ModelState is invalid.");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning("ModelState Error: {Error}", error.ErrorMessage);
                }
            }
        }

        _logger.LogInformation("Returning to page due to invalid state.");
        // If we got this far, something failed, redisplay form
        return Page();
    }

    private Planet CreateNewPlanetForUser(User user)
    {
        var rng = new Random();
        var galaxy = _context.Galaxies.Include(g => g.Systems).FirstOrDefault();
        var system = galaxy?.Systems.FirstOrDefault();

        if (galaxy == null || system == null)
        {
            throw new InvalidOperationException("No galaxy or system available.");
        }

        var planet = new Planet
        {
            Name = $"Planet {galaxy.Id}-{system.Id}-{rng.Next(1, 1000)}",
            System = system,
            RelativeSpeed = rng.Next(90, 111),
            RelativeIronOutput = rng.Next(90, 111),
            RelativeSilverOutput = rng.Next(90, 111),
            RelativeAluminiumOutput = rng.Next(90, 111),
            RelativeH2Output = rng.Next(90, 111),
            RelativeEnergyOutput = rng.Next(90, 111),
            Iron = 500,
            Silver = 500,
            Aluminium = 500,
            H2 = 500,
            Energy = 500,
            UserId = user.Id,
            User = user
        };

        _logger.LogInformation("New planet created: {Planet}", planet);

        return planet;
    }
}
