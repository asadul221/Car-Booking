using System;
using System.Globalization;

namespace Wafi.SampleTest.Dtos
{
    public class BookingFilterDto
    {
        public Guid CarId { get; set; }

        public string StartBookingDate { get; set; }  // Accept as string
        public string EndBookingDate { get; set; }    // Accept as string

        // Method to parse StartBookingDate safely
        public DateOnly GetStartBookingDate()
        {
            if (DateOnly.TryParseExact(StartBookingDate, "yyyy-MM-dd", out DateOnly parsedDate))
            {
                return parsedDate;
            }
            throw new FormatException($"Invalid StartBookingDate format: {StartBookingDate}. Expected format: yyyy-MM-dd");
        }

        // Method to parse EndBookingDate safely
        public DateOnly GetEndBookingDate()
        {
            if (DateOnly.TryParseExact(EndBookingDate, "yyyy-MM-dd", out DateOnly parsedDate))
            {
                return parsedDate;
            }
            throw new FormatException($"Invalid EndBookingDate format: {EndBookingDate}. Expected format: yyyy-MM-dd");
        }
    }
}
