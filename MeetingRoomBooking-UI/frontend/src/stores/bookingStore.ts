import { defineStore } from 'pinia'
import { ref } from 'vue'
import { bookingApi } from '../services/bookingApi'
import type { Booking, CreateBookingInput, Room } from '../types/booking'

export const useBookingStore = defineStore('booking', () => {
  const bookings = ref<Booking[]>([])
  const rooms = ref<Room[]>([])
  const isLoading = ref(false)

  async function load(): Promise<void> {
    isLoading.value = true
    try {
      const [loadedRooms, loadedBookings] = await Promise.all([
        bookingApi.getRooms(),
        bookingApi.getBookings(),
      ])
      rooms.value = loadedRooms
      bookings.value = loadedBookings
    } finally {
      isLoading.value = false
    }
  }

  async function createBooking(input: CreateBookingInput): Promise<Booking> {
    const booking = await bookingApi.createBooking(input)
    bookings.value = [...bookings.value, booking].sort(
      (first, second) => first.start.localeCompare(second.start),
    )
    return booking
  }

  async function updateBooking(id: string, input: CreateBookingInput): Promise<Booking> {
    const booking = await bookingApi.updateBooking(id, input)
    bookings.value = bookings.value
      .map((existing) => (existing.id === id ? booking : existing))
      .sort((first, second) => first.start.localeCompare(second.start))
    return booking
  }

  async function cancelBooking(id: string): Promise<void> {
    await bookingApi.cancelBooking(id)
    bookings.value = bookings.value.filter((booking) => booking.id !== id)
  }

  return { bookings, rooms, isLoading, load, createBooking, updateBooking, cancelBooking }
})
