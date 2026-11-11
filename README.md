# Meeting Room Booking

A full-stack meeting room booking application with a Vue 3 client and an ASP.NET Core API. The frontend uses the API for all room and booking data; SQLite provides persistent local storage.

## Solution structure

- `MeetingRoomBooking.Api` - HTTP controllers, OpenAPI, Scalar API reference, CORS, and SQLite startup.
- `MeetingRoomBooking.Application` - DTOs, service contracts, validation, and booking rules.
- `MeetingRoomBooking.Domain` - booking and meeting room entities.
- `MeetingRoomBooking.Infrastructure` - SQLite/EF Core persistence, repositories, and seeded rooms.
- `MeetingRoomBooking.Tests` - booking behavior and SQLite persistence tests.
- `MeetingRoomBooking-UI/frontend` - Vue 3, TypeScript, Pinia, and Zod client.

## Features

- Lists seeded meeting rooms and existing bookings from the API.
- Creates and edits bookings with frontend and backend validation.
- Prevents overlapping bookings for the same room.
- Cancels bookings with a confirmation and success message.
- Displays clear loading and validation states, plus accessible notifications for successful or failed actions.
- Uses SQLite at `MeetingRoomBooking.Api/App_Data/meeting-room-booking.db`; the database is created on first API startup and is excluded from Git.

Rooms are seeded when the database is first created. Booking data persists across API restarts. The app uses `EnsureCreated` to keep local setup simple; schema changes require recreating the local database until migrations are introduced.

## Run locally

Install the .NET 10 SDK and Node.js 22.18+.

Start the API from the solution root:

```powershell
dotnet restore
dotnet run --project MeetingRoomBooking.Api --launch-profile http
```

In Development, the API listens over HTTP at `http://localhost:5108` so the Vite client can call it without an HTTPS redirect. Interactive API documentation is available at [http://localhost:5108/scalar/v1](http://localhost:5108/scalar/v1), and the OpenAPI document is at [http://localhost:5108/openapi/v1.json](http://localhost:5108/openapi/v1.json). HTTPS redirection remains enabled outside Development.

In another terminal, start the Vue client:

```powershell
Set-Location MeetingRoomBooking-UI/frontend
npm install
npm run dev
```

Open the Vite URL printed in the terminal (normally `http://localhost:5173`). The API permits that development origin. To use another API URL, set `VITE_API_BASE_URL` to the API base URL, including `/api`, before starting or building the frontend.

## API endpoints

### Rooms

- `GET /api/rooms` - list rooms
- `GET /api/rooms/{id}` - get one room

### Bookings

- `GET /api/bookings` - list bookings
- `GET /api/bookings?roomId={id}` - list bookings for a room
- `GET /api/bookings/{id}` - get one booking
- `POST /api/bookings` - create a booking (`201 Created`)
- `PUT /api/bookings/{id}` - update a booking
- `DELETE /api/bookings/{id}` - cancel a booking (`204 No Content`)

Invalid booking data returns a structured `400` Problem Details response. A room-time conflict returns `409`, and missing resources return `404`.

## Run tests

```powershell
dotnet test MeetingRoomBooking.slnx
```

## Scope and assumptions

- Authentication, recurring meetings, attendee management, email notifications, and realtime updates are not included.
- Cancellation is implemented as deletion.
- Room data is seeded; there is no room administration interface.
- The frontend is a separate client and communicates with the API over HTTP.
