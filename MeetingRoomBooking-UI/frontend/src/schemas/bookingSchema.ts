import { z } from 'zod'
import type { CreateBookingInput } from '../types/booking'

const bookingSchema = z
  .object({
    roomId: z.string().regex(/^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/i, 'Select a meeting room.'),
    title: z.string().trim().min(1, 'Add a meeting name to continue.').max(120, 'Meeting name cannot exceed 120 characters.'),
    date: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, 'Choose a valid date.'),
    startTime: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d$/, 'Choose a valid start time.'),
    endTime: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d$/, 'Choose a valid end time.'),
  })
  .superRefine((booking, context) => {
    const [yearText, monthText, dayText] = booking.date.split('-')
    const year = Number(yearText)
    const month = Number(monthText)
    const day = Number(dayText)
    const date = new Date(year, month - 1, day)
    if (
      Number.isNaN(date.getTime()) ||
      date.getFullYear() !== year ||
      date.getMonth() !== month - 1 ||
      date.getDate() !== day
    ) {
      context.addIssue({ code: 'custom', message: 'Choose a valid date.', path: ['date'] })
      return
    }

    const start = new Date(`${booking.date}T${booking.startTime}:00`)
    const end = new Date(`${booking.date}T${booking.endTime}:00`)
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
      context.addIssue({ code: 'custom', message: 'Choose valid start and end times.', path: ['startTime'] })
      return
    }

    if (start >= end) {
      context.addIssue({ code: 'custom', message: 'The end time must be after the start time.', path: ['endTime'] })
    }
  })

export interface ValidationResult {
  valid: boolean
  message?: string
}

export function validateBooking(input: CreateBookingInput): ValidationResult {
  const result = bookingSchema.safeParse(input)
  return result.success
    ? { valid: true }
    : { valid: false, message: result.error.issues[0]?.message ?? 'Check the booking details and try again.' }
}
