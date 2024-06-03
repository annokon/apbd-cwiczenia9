using APBD.Data;
using APBD.Models;
using APBD.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace APBD.Controllers
{
    [ApiController]
    [Route("api/trips/{idTrip}/clients")]
    public class ClientTripsController : ControllerBase
    {
        private readonly ApbdContext _context;

        public ClientTripsController(ApbdContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientTripRequest request)
        {
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == request.Pesel);
            if (existingClient != null)
            {
                return BadRequest(new { message = "Client with this PESEL already exists" });
            }

            var trip = await _context.Trips.Include(t => t.ClientTrips).FirstOrDefaultAsync(t => t.IdTrip == idTrip);
            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            if (trip.DateFrom <= DateTime.Now)
            {
                return BadRequest(new { message = "Cannot register for a trip that has already started or finished" });
            }

            if (trip.ClientTrips.Any(ct => ct.IdClientNavigation.Pesel == request.Pesel))
            {
                return BadRequest(new { message = "Client is already registered for this trip" });
            }

            var newClient = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Telephone = request.Telephone,
                Pesel = request.Pesel
            };

            var clientTrip = new ClientTrip
            {
                Trip = trip,
                Client = newClient,
                RegisteredAt = DateTime.Now,
                PaymentDate = request.PaymentDate
            };

            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client successfully registered to the trip" });
        }
    }
}
