# Meeting Room Booking UI

Vue 3 and TypeScript frontend for the Meeting Room Booking API.

## Setup

```powershell
npm install
npm run dev
```

Start the .NET API first (`dotnet run --project MeetingRoomBooking.Api --launch-profile http` from the solution root). The frontend defaults to `http://localhost:5108/api`; set `VITE_API_BASE_URL` to a different API base URL if needed.

## Validation and state

- Pinia manages rooms and bookings loaded from the API.
- Zod validates booking input before submission.
- The API remains authoritative for room existence and booking conflicts.

## Production build

```powershell
npm run build
```
