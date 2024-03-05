using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeminarHub.Data;
using SeminarHub.Data.Models;
using SeminarHub.Models;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Claims;
using static SeminarHub.Data.DataConstants;

namespace SeminarHub.Controllers
{
    [Authorize]
    public class SeminarController : Controller
    {
        private readonly SeminarHubDbContext data;

        public SeminarController(SeminarHubDbContext context)
        {
            data = context;
        }
        public async Task<IActionResult> All()
        {
            var models = await data.Seminars.Select(x => new AllViewModel()
            {
                Id = x.Id,
                Topic = x.Topic,
                Lecturer = x.Lecturer,
                Category = x.Category.Name,
                DateAndTime = x.DateAndTime.ToString(SeminarDateAndTimeFormat),
                Organizer = x.Organizer.UserName
            })
            .ToListAsync();

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new AddViewModel();
            model.Categories = await data.Categories.Select(x => new CategoryViewModel()
            {
                Id = x.Id,
                Name = x.Name
            })
                .ToListAsync();

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Add(AddViewModel model)
        {
            DateTime StartingTime = DateTime.Now;

            if (!DateTime.TryParseExact(model.DateAndTime, SeminarDateAndTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out StartingTime))
            {
                ModelState.AddModelError(nameof(model.DateAndTime), $"Invalid date, format must be {SeminarDateAndTimeFormat}");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await data.Categories.Select(x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

                return View(model);
            }

            var seminar = new Seminar()
            {
                Topic = model.Topic,
                Lecturer = model.Lecturer,
                Details = model.Details,
                OrganizerId = GetUserId(),
                DateAndTime = StartingTime,
                CategoryId = model.CategoryId,
                Duration = int.Parse(model.Duration)

            };

            data.Seminars.Add(seminar);

            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> Joined()
        {
            var userId = GetUserId();

            var models = await data.Seminars
                .Where(x => x.SeminarsParticipants.Any(x => x.ParticipantId == userId))
                .Select(x => new AllViewModel()
                {
                    Id = x.Id,
                    Topic = x.Topic,
                    Lecturer = x.Lecturer,
                    Category = x.Category.Name,
                    DateAndTime = x.DateAndTime.ToString(SeminarDateAndTimeFormat),
                    Organizer = x.Organizer.UserName
                })
                .ToListAsync();

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await data.Seminars.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (service == null)
            {
                return BadRequest();
            }

            if (service.OrganizerId != GetUserId())
            {
                return Unauthorized();
            }

            var neededModel = new EditViewModel()
            {
                Id = service.Id,
                Topic = service.Topic,
                Details = service.Details,
                DateAndTime = service.DateAndTime.ToString(SeminarDateAndTimeFormat),
                Lecturer = service.Lecturer,
                Duration = service.Duration,
                CategoryId = service.CategoryId
            };

            neededModel.Categories = await data.Categories.Select(x => new CategoryViewModel()
            {
                Id = x.Id,
                Name = x.Name
            })
                .ToListAsync();

            return View(neededModel);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(EditViewModel model)
        {
            DateTime StartingTime = DateTime.Now;

            if (!DateTime.TryParseExact(model.DateAndTime, SeminarDateAndTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out StartingTime))
            {
                ModelState.AddModelError(nameof(model.DateAndTime), $"Invalid date, format must be {SeminarDateAndTimeFormat}");
            }

            var neededSeminar = await data.Seminars
                .Where(x => x.Id == model.Id)
                .FirstOrDefaultAsync();

            if (neededSeminar == null)
            {
                return BadRequest();
            }

            if (neededSeminar.OrganizerId != GetUserId())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await data.Categories.Select(x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

                return View(model);
            }

            neededSeminar.Details = model.Details;
            neededSeminar.Topic = model.Topic;
            neededSeminar.Lecturer = model.Lecturer;
            neededSeminar.DateAndTime = StartingTime;
            neededSeminar.Duration = model.Duration;
            neededSeminar.CategoryId = model.CategoryId;

            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> Join(int id)
        {
            var userId = GetUserId();

            var model = await data.Seminars
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return BadRequest();
            }

            if (!data.SeminarParticipants.Any(x => x.SeminarId == id && x.ParticipantId == userId))
            {
                var seminarParticipant = new SeminarParticipant()
                {
                    ParticipantId = userId,
                    SeminarId = id
                };

                data.SeminarParticipants.Add(seminarParticipant);

                await data.SaveChangesAsync();

                return RedirectToAction(nameof(Joined));
            }

            return RedirectToAction(nameof(All));

        }

        public async Task<IActionResult> Leave(int id)
        {
            var userId = GetUserId();

            var seminarParticipant = await data.SeminarParticipants
                .Where(x => x.SeminarId == id && x.ParticipantId == userId)
                .FirstOrDefaultAsync();

            if (seminarParticipant == null)
            {
                return BadRequest();
            }

            data.SeminarParticipants.Remove(seminarParticipant);

            await data.SaveChangesAsync();

            return RedirectToAction(nameof(Joined));
        }

        public async Task<IActionResult> Details(int id)
        {
            var seminar = await data.Seminars
                .Where(x => x.Id == id)
                .Select(x => new DetailsViewModel()
                {
                    Id = x.Id,
                    Topic = x.Topic,
                    Details = x.Details,
                    DateAndTime = x.DateAndTime.ToString(SeminarDateAndTimeFormat),
                    Lecturer = x.Lecturer,
                    Category = x.Category.Name,
                    Duration = x.Duration.ToString(),
                    Organizer = x.Organizer.UserName
                })
                .FirstOrDefaultAsync();



            return View(seminar);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var seminar = await data.Seminars
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (seminar == null)
            {
                return BadRequest();
            }

            if (seminar.OrganizerId != userId)
            {
                return Unauthorized();
            }

            var neededModel = new DeleteViewModel()
            {
                Id = seminar.Id,
                Topic = seminar.Topic,
                DateAndTime = seminar.DateAndTime
            };

            return View(neededModel);
        }

        [HttpPost]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seminar = await data.Seminars.FindAsync(id);

            if (seminar == null)
            {
                return NotFound();
            }

            data.Seminars.Remove(seminar);
            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
    }
}
