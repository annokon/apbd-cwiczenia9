using APBD.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var trips = await _context.Trips
            .OrderByDescending(e => e.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Name,
                e.Description,
                DateFrom = e.DateFrom.ToString("yyyy-MM-dd"),
                DateTo = e.DateTo.ToString("yyyy-MM-dd"),
                e.MaxPeople,
                Countries = e.IdCountries.Select(c => new { c.Name }).ToList(),
                Clients = e.ClientTrips.Select(ct => new { ct.IdClientNavigation.FirstName, ct.IdClientNavigation.LastName }).ToList()
            }).ToListAsync();

        var result = new
        {
            pageNum = page,
            pageSize,
            allPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            trips
        };

        return Ok(result);
    }
}