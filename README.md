# MeetingRoomBooking API

A .NET backend for managing meeting room bookings in a small office environment. The project is intentionally focused on the API layer and follows a layered design so the business logic remains independent from the web layer and in-memory data access.

## Architecture

The solution is structured into four clear layers:

- `MeetingRoomBooking.Api` – ASP.NET Core controllers and HTTP endpoints.
- `MeetingRoomBooking.Application` – application contracts, DTOs, service logic, and validation rules.
- `MeetingRoomBooking.Domain` – core entities such as meeting rooms and bookings.
- `MeetingRoomBooking.Infrastructure` – in-memory repositories and seeded sample data.

This keeps the API thin, makes validation logic reusable, and allows the data source or front-end client to change without rewriting the booking rules.

## Features

- Seeded meeting rooms with capacity details.
- View all rooms and room-specific schedules.
- Create, update, and delete bookings.
- Prevent overlapping bookings for the same room.
- Validate required fields and time ranges before accepting requests.
- Return clear validation feedback in API responses.

## API Endpoints

The API exposes OpenAPI metadata and can be explored at `/openapi/v1.json` while the app is running.

### Rooms

- `GET /api/rooms` – list all rooms
- `GET /api/rooms/{id}` – get one room by id

### Bookings

- `GET /api/bookings` – list all bookings or filter by room with `?roomId={id}`
- `GET /api/bookings/{id}` – get one booking by id
- `POST /api/bookings` – create a booking
- `PUT /api/bookings/{id}` – update a booking
- `DELETE /api/bookings/{id}` – cancel/delete a booking

## Example Request

```http
POST https://localhost:5001/api/bookings
Content-Type: application/json

{
  "roomId": "11111111-1111-1111-1111-111111111111",
  "title": "Design review",
  "start": "2026-11-11T09:00:00Z",
  "end": "2026-11-11T10:00:00Z"
}
```

## Validation Rules

The backend rejects requests when:

- the room does not exist
- the title is empty
- the start time is missing or invalid
- the end time is missing or invalid
- the end time is not after the start time
- another booking overlaps in the same room

## Getting Started

1. Ensure the .NET SDK is installed.
2. Restore dependencies:

```bash
dotnet restore
```

3. Run the API:

```bash
dotnet run --project MeetingRoomBooking.Api
```

4. Open the API documentation in a browser via the generated OpenAPI endpoint, or call the API using HTTP tooling.

## Running the Tests

```bash
dotnet test MeetingRoomBooking.slnx --nologo
```

## Notes

This repository currently focuses on the backend API for the booking workflow, in line with the assessment requirements. The frontend layer is intentionally not included in this iteration so the API remains independent, testable, and reusable for a future Vue client.
