using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using Wafi.SampleTest.Dtos;
using Wafi.SampleTest.Entities;

namespace Wafi.SampleTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly WafiDbContext _context;
        public BookingsController(WafiDbContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet("Booking")]
        public async Task<IEnumerable<BookingCalendarDto>> GetCalendarBookings([FromQuery] BookingFilterDto input)
        {
            DateOnly startDate = input.GetStartBookingDate();
            DateOnly endDate = input.GetEndBookingDate();

            var bookings = await _context.Bookings
            .Include(b => b.Car)
            .Where(b => b.CarId == input.CarId &&
            EF.Functions.DateDiffDay(b.BookingDate, startDate) <= 0 &&
            EF.Functions.DateDiffDay(b.BookingDate, endDate) >= 0)
            .ToListAsync();

            // TO DO: convert the database bookings to calendar view (date, start time, end time).
            // Consiser NoRepeat, Daily and Weekly options

            var calendar = new Dictionary<DateOnly, List<Booking>>();

            foreach (var booking in bookings)
            {
                var currentDate = booking.BookingDate;
                while (currentDate <= (booking.EndRepeatDate ?? booking.BookingDate))
                {
                    if (!calendar.ContainsKey(currentDate))
                        calendar[currentDate] = new List<Booking>();

                    calendar[currentDate].Add(booking);

                    currentDate = booking.RepeatOption switch
                    {
                        RepeatOption.Daily => currentDate.AddDays(1),
                        RepeatOption.Weekly => currentDate.AddDays(7),
                        _ => booking.EndRepeatDate.HasValue ? booking.EndRepeatDate.Value.AddDays(1) : currentDate.AddDays(1)
                    };
                }
            }

            List<BookingCalendarDto> result = new List<BookingCalendarDto>();

            foreach (var item in calendar)
            {
                foreach (var booking in item.Value)
                {
                    result.Add(new BookingCalendarDto { BookingDate = item.Key, CarModel = booking.Car.Model, StartTime = booking.StartTime, EndTime = booking.EndTime });
                }
            }

            return result;
        }

        // POST: api/Bookings
        [HttpPost("Booking")]
        public async Task<IActionResult> PostBooking(CreateUpdateBookingDto bookingDto)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly bookingDate, endRepeatDate;

            try
            {
                bookingDate = bookingDto.GetBookingDate();
                endRepeatDate = bookingDto.GetEndRepeatDate() ?? bookingDate;
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            if (bookingDate < DateOnly.FromDateTime(DateTime.UtcNow) ||
                (bookingDate == DateOnly.FromDateTime(DateTime.UtcNow) && bookingDto.StartTime < DateTime.UtcNow.TimeOfDay))
            {
                return BadRequest("You cannot book a car for a past date or time.");
            }




            // TO DO: Validate if any booking time conflicts with existing data. Return error if any conflicts
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (bookingDto.StartTime >= bookingDto.EndTime)
            {
                return BadRequest("Start time must be before end time.");
            }


            // load all bookings for specified car
            var bookingListOfTargetCar = await _context.Bookings.Where(b => b.CarId ==  bookingDto.CarId).ToListAsync();

            // Check if booking has collision
            if (HasBookingCollision(bookingListOfTargetCar, bookingDto))
            {
                return BadRequest("There is a collision with existing booking for this car, please try again!");
            }

            var newBooking = new Booking
            {
                Id = Guid.NewGuid(),
                BookingDate = bookingDate, 
                StartTime = bookingDto.StartTime,
                EndTime = bookingDto.EndTime,
                RepeatOption = bookingDto.RepeatOption,
                EndRepeatDate = bookingDto.GetEndRepeatDate(), 
                CarId = bookingDto.CarId,
            };


            await _context.Bookings.AddAsync(newBooking);
            await _context.SaveChangesAsync();
            return Ok(newBooking);
        }

        private bool HasBookingCollision(List<Booking> bookingListOfTargetCar, CreateUpdateBookingDto bookingDto)
        {
            int repeatDays = bookingDto.RepeatOption switch
            {
                RepeatOption.Daily => 1,
                RepeatOption.Weekly => 7,
                _ => 0
            };

            List<DateOnly> newBookingDates = new List<DateOnly>();

            DateOnly start = bookingDto.GetBookingDate();
            DateOnly? endRepeatDate = bookingDto.GetEndRepeatDate();

            while (endRepeatDate != null && start <= endRepeatDate.Value)
            {
                newBookingDates.Add(start);

                if (repeatDays == 0) break;

                start = start.AddDays(repeatDays);
            }

            if (repeatDays == 0 || endRepeatDate == null)
            {
                newBookingDates.Add(bookingDto.GetBookingDate());
            }

            // Checking collision
            foreach (var existingBooking in bookingListOfTargetCar)
            {
                foreach (var newBookingDate in newBookingDates)
                {
                    if (newBookingDate == existingBooking.BookingDate)
                    {
                        if (!(bookingDto.EndTime <= existingBooking.StartTime || bookingDto.StartTime >= existingBooking.EndTime))
                        {
                            return true; // Collision detected
                        }
                    }
                }
            }
            return false; // No collision
        }


        // GET: api/SeedData
        // For test purpose
        [HttpGet("SeedData")]
        public async Task<IEnumerable<BookingCalendarDto>> GetSeedData()
        {
            var cars = await _context.Cars.ToListAsync();

            if (!cars.Any())
            {
                cars = GetCars().ToList();
                await _context.Cars.AddRangeAsync(cars);
                await _context.SaveChangesAsync();
            }

            var bookings = await _context.Bookings.ToListAsync();

            if(!bookings.Any())
            {
                bookings = GetBookings().ToList();

                await _context.Bookings.AddRangeAsync(bookings);
                await _context.SaveChangesAsync();
            }

            var calendar = new Dictionary<DateOnly, List<Booking>>();

            foreach (var booking in bookings)
            {
                var currentDate = booking.BookingDate;
                while (currentDate <= (booking.EndRepeatDate ?? booking.BookingDate))
                {
                    if (!calendar.ContainsKey(currentDate))
                        calendar[currentDate] = new List<Booking>();

                    calendar[currentDate].Add(booking);

                    currentDate = booking.RepeatOption switch
                    {
                        RepeatOption.Daily => currentDate.AddDays(1),
                        RepeatOption.Weekly => currentDate.AddDays(7),
                        _ => booking.EndRepeatDate.HasValue ? booking.EndRepeatDate.Value.AddDays(1) : currentDate.AddDays(1)
                    };
                }
            }

            List<BookingCalendarDto> result = new List<BookingCalendarDto>();

            foreach (var item in calendar)
            {
                foreach(var booking in item.Value)
                {
                    result.Add(new BookingCalendarDto { BookingDate = item.Key, CarModel = booking.Car.Model, StartTime = booking.StartTime, EndTime = booking.EndTime });
                }
            }

            return result;
        }

        #region Sample Data

        private IList<Car> GetCars()
        {
            var cars = new List<Car>
            {
                new Car { Id = Guid.NewGuid(), Make = "Toyota", Model = "Corolla" },
                new Car { Id = Guid.NewGuid(), Make = "Honda", Model = "Civic" },
                new Car { Id = Guid.NewGuid(), Make = "Ford", Model = "Focus" }
            };

            return cars;
        }

        private IList<Booking> GetBookings()
        {
            var cars = GetCars();

            var bookings = new List<Booking>
            {
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 2, 5), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), RepeatOption = RepeatOption.DoesNotRepeat, RequestedOn = DateTime.Now, CarId = cars[0].Id, Car = cars[0] },
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 2, 10), StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), RepeatOption = RepeatOption.Daily, EndRepeatDate = new DateOnly(2025, 2, 20), RequestedOn = DateTime.Now, CarId = cars[1].Id, Car = cars[1] },
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 2, 15), StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), RepeatOption = RepeatOption.Weekly, EndRepeatDate = new DateOnly(2025, 3, 31), RequestedOn = DateTime.Now, DaysToRepeatOn = DaysOfWeek.Monday, CarId = cars[2].Id,  Car = cars[2] },
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 3, 1), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(13, 0, 0), RepeatOption = RepeatOption.DoesNotRepeat, RequestedOn = DateTime.Now, CarId = cars[0].Id, Car = cars[0] },
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 3, 7), StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0), RepeatOption = RepeatOption.Weekly, EndRepeatDate = new DateOnly(2025, 3, 28), RequestedOn = DateTime.Now, DaysToRepeatOn = DaysOfWeek.Friday, CarId = cars[1].Id, Car = cars[1] },
                new Booking { Id = Guid.NewGuid(), BookingDate = new DateOnly(2025, 3, 15), StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(17, 0, 0), RepeatOption = RepeatOption.Daily, EndRepeatDate = new DateOnly(2025, 3, 20), RequestedOn = DateTime.Now, CarId = cars[2].Id,  Car = cars[2] }
            };

            return bookings;
        }

            #endregion
        }
}
