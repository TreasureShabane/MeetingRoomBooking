export interface Room {
  id: string
  name: string
  capacity: number
}

export interface Booking {
  id: string
  roomId: string
  roomName: string
  title: string
  start: string
  end: string
}

export interface CreateBookingInput {
  roomId: string
  title: string
  date: string
  startTime: string
  endTime: string
}

export interface BookingRequest {
  roomId: string
  title: string
  start: string
  end: string
}
