<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
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

function toDateInputValue(value: Date): string {
  const localDate = new Date(value.getTime() - value.getTimezoneOffset() * 60_000)
  return localDate.toISOString().slice(0, 10)
}

function toTimeInputValue(value: Date): string {
  return `${String(value.getHours()).padStart(2, '0')}:${String(value.getMinutes()).padStart(2, '0')}`
}

function getNextAvailableTime(value: Date): string | null {
  const nextMinute = new Date(value)
  if (nextMinute.getSeconds() > 0 || nextMinute.getMilliseconds() > 0) {
    nextMinute.setMinutes(nextMinute.getMinutes() + 1)
  }
  nextMinute.setSeconds(0, 0)

  return toDateInputValue(nextMinute) === toDateInputValue(value)
    ? toTimeInputValue(nextMinute)
    : null
}

function oneHourAfter(time: string): string {
  const [hours = '0', minutes = '0'] = time.split(':')
  const endMinutes = Math.min(Number(hours) * 60 + Number(minutes) + 60, 23 * 60 + 59)
  return `${String(Math.floor(endMinutes / 60)).padStart(2, '0')}:${String(endMinutes % 60).padStart(2, '0')}`
}

const currentTime = ref(new Date())
const initialBookingStart = props.booking ? toLocalDateTimeInput(props.booking.start) : null
const initialBookingEnd = props.booking ? toLocalDateTimeInput(props.booking.end) : null
const date = ref(initialBookingStart?.slice(0, 10) ?? props.initialDate)
const initialStartTime =
  initialBookingStart?.slice(11, 16) ??
  (date.value === toDateInputValue(currentTime.value)
    ? (getNextAvailableTime(currentTime.value) ?? '23:59')
    : '10:00')
const title = ref(props.booking?.title ?? '')
const roomId = ref(props.booking?.roomId ?? props.initialRoomId)
const startTime = ref(initialStartTime)
const endTime = ref(
  initialBookingEnd?.slice(11, 16) ??
    (date.value === toDateInputValue(currentTime.value) ? oneHourAfter(initialStartTime) : '11:00'),
)
const validationError = ref('')

const selectedRoom = computed(() => props.rooms.find((room) => room.id === roomId.value))
const preservingPastBooking = computed(() => {
  return (
    props.booking !== null &&
    date.value === initialBookingStart?.slice(0, 10) &&
    startTime.value === initialBookingStart?.slice(11, 16) &&
    endTime.value === initialBookingEnd?.slice(11, 16) &&
    date.value === toDateInputValue(currentTime.value) &&
    new Date(props.booking.start).getTime() < currentTime.value.getTime()
  )
})
const noTimesRemainingToday = computed(() => {
  if (date.value !== toDateInputValue(currentTime.value) || preservingPastBooking.value) {
    return false
  }

  const nextAvailable = getNextAvailableTime(currentTime.value)
  return nextAvailable === null || nextAvailable >= '23:59'
})
const minimumStartTime = computed(() => {
  if (date.value !== toDateInputValue(currentTime.value) || preservingPastBooking.value || noTimesRemainingToday.value) {
    return undefined
  }
  return getNextAvailableTime(currentTime.value) ?? undefined
})
const minimumEndTime = computed(() => startTime.value)

watch(minimumStartTime, (minimum) => {
  if (minimum && startTime.value < minimum) {
    startTime.value = minimum
    if (endTime.value <= minimum) {
      endTime.value = oneHourAfter(minimum)
    }
  }
})

watch(startTime, (start) => {
  if (endTime.value <= start) {
    endTime.value = oneHourAfter(start)
  }
})

let clockInterval: number | undefined
onMounted(() => {
  clockInterval = window.setInterval(() => {
    currentTime.value = new Date()
  }, 30_000)
})
onBeforeUnmount(() => {
  if (clockInterval !== undefined) {
    window.clearInterval(clockInterval)
  }
})

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
  <section class="booking-form-page" aria-labelledby="dialog-title" @keydown.esc="emit('close')">
    <header class="dialog-header">
      <div>
        <span class="dialog-eyebrow">MAKE IT A GOOD MEETING</span>
        <h1 id="dialog-title">{{ booking ? 'Edit booking' : 'Book a room' }}</h1>
        <p>Choose a room and meeting time. Past times are unavailable for today.</p>
      </div>
      <button class="dialog-close" type="button" aria-label="Back to previous screen" @click="emit('close')">←</button>
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
            <input v-model="startTime" type="time" :min="minimumStartTime" :disabled="noTimesRemainingToday" required />
          </label>
          <label class="form-field">
            <span>Ends</span>
            <input v-model="endTime" type="time" :min="minimumEndTime" :disabled="noTimesRemainingToday" required />
          </label>
        </div>

        <p v-if="validationError || submissionError || noTimesRemainingToday" class="form-error" role="alert">
          {{ validationError || submissionError || 'No booking times remain today. Choose another date.' }}
        </p>

        <div class="dialog-actions">
          <button class="button button-secondary" type="button" @click="emit('close')">Cancel</button>
          <button class="button button-primary" type="submit" :disabled="noTimesRemainingToday">
            {{ booking ? 'Save changes' : 'Confirm booking' }} <span aria-hidden="true">→</span>
          </button>
        </div>
        <p class="form-footnote">Bookings cannot overlap in the same room.</p>
    </form>
  </section>
</template>
