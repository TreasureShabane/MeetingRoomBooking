<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Booking, CreateBookingInput, Room } from '../types/booking'
import { validateBooking } from '../schemas/bookingSchema'

const props = defineProps<{
  rooms: Room[]
  initialDate: string
  initialRoomId: string
  booking: Booking | null
  submissionError: string
}>()

const emit = defineEmits<{
  close: []
  book: [booking: CreateBookingInput]
}>()

function toLocalDateTimeInput(value: string): string {
  const date = new Date(value)
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return localDate.toISOString().slice(0, 16)
}

const title = ref(props.booking?.title ?? '')
const roomId = ref(props.booking?.roomId ?? props.initialRoomId)
const date = ref(props.booking ? toLocalDateTimeInput(props.booking.start).slice(0, 10) : props.initialDate)
const startTime = ref(props.booking ? toLocalDateTimeInput(props.booking.start).slice(11, 16) : '10:00')
const endTime = ref(props.booking ? toLocalDateTimeInput(props.booking.end).slice(11, 16) : '11:00')
const validationError = ref('')

const selectedRoom = computed(() => props.rooms.find((room) => room.id === roomId.value))

function submitBooking(): void {
  const booking: CreateBookingInput = {
    title: title.value,
    roomId: roomId.value,
    date: date.value,
    startTime: startTime.value,
    endTime: endTime.value,
  }
  const validation = validateBooking(booking)
  if (!validation.valid) {
    validationError.value = validation.message ?? 'Check the booking details and try again.'
    return
  }

  validationError.value = ''
  emit('book', booking)
}
</script>

<template>
  <div class="dialog-backdrop" @click.self="emit('close')" @keydown.esc="emit('close')">
    <section
      class="booking-dialog"
      role="dialog"
      aria-modal="true"
      aria-labelledby="dialog-title"
    >
      <header class="dialog-header">
        <div>
          <span class="dialog-eyebrow">MAKE IT A GOOD MEETING</span>
          <h2 id="dialog-title">{{ booking ? 'Edit booking' : 'Book a room' }}</h2>
          <p>Choose a room and meeting time.</p>
        </div>
        <button class="dialog-close" type="button" aria-label="Close dialog" @click="emit('close')">×</button>
      </header>

      <form class="booking-form" @submit.prevent="submitBooking">
        <label class="form-field">
          <span>Meeting name</span>
          <input v-model="title" autofocus maxlength="120" placeholder="e.g. Weekly team sync" required />
        </label>

        <label class="form-field">
          <span>Meeting room</span>
          <select v-model="roomId" required>
            <option v-for="room in rooms" :key="room.id" :value="room.id">
              {{ room.name }} · {{ room.capacity }} people
            </option>
          </select>
        </label>

        <div v-if="selectedRoom" class="selected-room-note">
          <span class="selected-room-icon" aria-hidden="true">⌂</span>
          <span>Capacity: {{ selectedRoom.capacity }} people</span>
        </div>

        <label class="form-field">
          <span>Date</span>
          <input v-model="date" type="date" required />
        </label>

        <div class="form-row">
          <label class="form-field">
            <span>Starts</span>
            <input v-model="startTime" type="time" required />
          </label>
          <label class="form-field">
            <span>Ends</span>
            <input v-model="endTime" type="time" required />
          </label>
        </div>

        <p v-if="validationError || submissionError" class="form-error" role="alert">
          {{ validationError || submissionError }}
        </p>

        <div class="dialog-actions">
          <button class="button button-secondary" type="button" @click="emit('close')">Cancel</button>
          <button class="button button-primary" type="submit">
            {{ booking ? 'Save changes' : 'Confirm booking' }} <span aria-hidden="true">→</span>
          </button>
        </div>
        <p class="form-footnote">Bookings cannot overlap in the same room.</p>
      </form>
    </section>
  </div>
</template>
