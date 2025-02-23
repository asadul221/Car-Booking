using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Wafi.SampleTest.Entities;

namespace Wafi.SampleTest.Dtos
{
    public class CreateUpdateBookingDto
    {
        public Guid Id { get; set; }

        [Required]
        [JsonPropertyName("bookingDate")]
        public string BookingDate { get; set; }  

        [Required]
        [JsonPropertyName("startTime")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [JsonPropertyName("endTime")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [JsonPropertyName("repeatOption")]
        public RepeatOption RepeatOption { get; set; }

        [JsonPropertyName("endRepeatDate")]
        public string? EndRepeatDate { get; set; } 

        [JsonPropertyName("requestedOn")]
        public DateTime RequestedOn { get; set; } = DateTime.UtcNow;

        [Required]
        [JsonPropertyName("carId")]
        public Guid CarId { get; set; }

        // Convert BookingDate & EndRepeatDate to DateOnly in Controller
        public DateOnly GetBookingDate()
        {
            if (DateOnly.TryParseExact(BookingDate, "yyyy-MM-dd", out var parsedDate))
            {
                return parsedDate;
            }
            throw new FormatException($"Invalid BookingDate format: {BookingDate}. Expected format: yyyy-MM-dd.");
        }

        public DateOnly? GetEndRepeatDate()
        {
            if (string.IsNullOrEmpty(EndRepeatDate)) return null;
            if (DateOnly.TryParseExact(EndRepeatDate, "yyyy-MM-dd", out var parsedDate))
            {
                return parsedDate;
            }
            throw new FormatException($"Invalid EndRepeatDate format: {EndRepeatDate}. Expected format: yyyy-MM-dd.");
        }
    }
}
