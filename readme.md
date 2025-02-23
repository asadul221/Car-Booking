```bash
git clone git@github.com:asadul221/Car-Booking.git
```



## Methods

GET: `$host:7201/api/bookings/booking`

#### For fetching exact car booking details 
```bash
curl -X 'GET' \
  'https://localhost:7201/api/Bookings/Booking?CarId=<car-id>&StartBookingDate=<start-date>&EndBookingDate=<end-date>' \
  -H 'accept: text/plain'
```
#### Example
```bash
curl -X 'GET' \
  'https://localhost:7201/api/Bookings/Booking?CarId=E6363DD2-A141-4D46-AB48-19B541CABCDF&StartBookingDate=2025-03-03&EndBookingDate=2025-03-30' \
  -H 'accept: text/plain'
```
`NB:` Need to handle the trailing zero

`Date format: year-month-day`<br/>

`Example: 2025-02-10`


### Request Body
```json

```

### For new booking

POST: `$host:7201/api/bookings/booking`

### Request Body
```json
{
  "bookingDate": "2025-03-10",
  "startTime": "10:00:00",
  "endTime": "12:00:00",
  "repeatOption": 1,
  "endRepeatDate": "2025-03-20",
  "requestedOn": "2025-02-23T09:29:14.290Z",
  "carId": "E6363DD2-A141-4D46-AB48-19B541CABCDF"
}
```

GET: `$host:7201/api/bookings/SeedData`

This method is for making database schema. (First Time run only)
