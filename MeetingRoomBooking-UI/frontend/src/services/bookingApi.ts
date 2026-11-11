import type { Booking, BookingRequest, CreateBookingInput, Room } from '../types/booking'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5108/api').replace(/\/$/, '')

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

async function getErrorMessage(response: Response): Promise<string> {
  const payload: unknown = await response.json()
  if (!isRecord(payload)) {
    return `The request failed (${response.status}).`
  }

  const errors = payload.errors
  if (isRecord(errors)) {
    const messages = Object.values(errors)
      .flatMap((value) => (Array.isArray(value) ? value : []))
      .filter((value): value is string => typeof value === 'string')
    if (messages.length) {
      return messages.join(' ')
    }
  }

  if (Array.isArray(errors)) {
    const messages = errors.filter((value): value is string => typeof value === 'string')
    if (messages.length) {
      return messages.join(' ')
    }
  }

  const detail = payload.detail
  const title = payload.title
  if (typeof detail === 'string' && detail) {
    return detail
  }
  if (typeof title === 'string' && title) {
    return title
  }

  return `The request failed (${response.status}).`
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...init?.headers,
    },
  })

  if (!response.ok) {
    throw new ApiError(await getErrorMessage(response), response.status)
  }
  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

function toRequest(input: CreateBookingInput): BookingRequest {
  return {
    roomId: input.roomId,
    title: input.title.trim(),
    start: new Date(`${input.date}T${input.startTime}:00`).toISOString(),
    end: new Date(`${input.date}T${input.endTime}:00`).toISOString(),
  }
}

export const bookingApi = {
  getRooms(): Promise<Room[]> {
    return request<Room[]>('/rooms')
  },

  getBookings(): Promise<Booking[]> {
    return request<Booking[]>('/bookings')
  },

  createBooking(input: CreateBookingInput): Promise<Booking> {
    return request<Booking>('/bookings', {
      method: 'POST',
      body: JSON.stringify(toRequest(input)),
    })
  },

  updateBooking(id: string, input: CreateBookingInput): Promise<Booking> {
    return request<Booking>(`/bookings/${encodeURIComponent(id)}`, {
      method: 'PUT',
      body: JSON.stringify(toRequest(input)),
    })
  },

  cancelBooking(id: string): Promise<void> {
    return request<void>(`/bookings/${encodeURIComponent(id)}`, { method: 'DELETE' })
  },
}
