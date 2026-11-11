<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import BookingDialog from './components/BookingDialog.vue'
import { useBookingStore } from './stores/bookingStore'
import type { Booking, CreateBookingInput } from './types/booking'

const store = useBookingStore()
const { bookings, rooms, isLoading } = storeToRefs(store)
const selectedDate = ref(toDateInputValue(new Date()))
const searchQuery = ref('')
const capacityFilter = ref('any')
const showBookingDialog = ref(false)
const activeNav = ref('Overview')
const editingBooking = ref<Booking | null>(null)
const bookingRoomId = ref('')
const dialogError = ref('')
const loadError = ref('')
const notice = ref<{ kind: 'success' | 'error'; message: string } | null>(null)
let noticeTimeout: number | undefined

const filteredRooms = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return rooms.value.filter((room) => {
    const matchesQuery = !query || room.name.toLowerCase().includes(query)
    const matchesCapacity =
      capacityFilter.value === 'any' || room.capacity >= Number(capacityFilter.value)
    return matchesQuery && matchesCapacity
  })
})

const todaysBookings = computed(() =>
  bookings.value
    .filter((booking) => toDateInputValue(new Date(booking.start)) === selectedDate.value)
    .sort((first, second) => first.start.localeCompare(second.start)),
)

const roomsWithoutBookings = computed(() => {
  const bookedRoomIds = new Set(todaysBookings.value.map((booking) => booking.roomId))
  return rooms.value.length - bookedRoomIds.size
})

const formattedDate = computed(() =>
  new Intl.DateTimeFormat('en', { weekday: 'long', month: 'long', day: 'numeric' }).format(
    new Date(`${selectedDate.value}T12:00:00`),
  ),
)

onMounted(async () => {
  try {
    await store.load()
  } catch (error) {
    loadError.value = getErrorMessage(error)
  }
})

onBeforeUnmount(dismissNotice)

function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong. Please try again.'
}

function dismissNotice(): void {
  if (noticeTimeout !== undefined) {
    window.clearTimeout(noticeTimeout)
    noticeTimeout = undefined
  }
  notice.value = null
}

function showNotice(kind: 'success' | 'error', message: string): void {
  dismissNotice()
  notice.value = { kind, message }
  noticeTimeout = window.setTimeout(dismissNotice, 6000)
}

function toDateInputValue(date: Date): string {
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return localDate.toISOString().slice(0, 10)
}

function changeDate(offset: number): void {
  const date = new Date(`${selectedDate.value}T12:00:00`)
  date.setDate(date.getDate() + offset)
  selectedDate.value = toDateInputValue(date)
}

function formatTime(dateTime: string): string {
  return new Intl.DateTimeFormat('en', { hour: 'numeric', minute: '2-digit' }).format(
    new Date(dateTime),
  )
}

function formatShortDate(date: string): string {
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric' }).format(
    new Date(`${date}T12:00:00`),
  )
}

function roomBookingCount(roomId: string): number {
  return todaysBookings.value.filter((booking) => booking.roomId === roomId).length
}

function openBookingDialog(roomId = rooms.value[0]?.id ?? ''): void {
  editingBooking.value = null
  bookingRoomId.value = roomId
  dialogError.value = ''
  dismissNotice()
  showBookingDialog.value = true
}

function openEditDialog(booking: Booking): void {
  editingBooking.value = booking
  bookingRoomId.value = booking.roomId
  dialogError.value = ''
  dismissNotice()
  showBookingDialog.value = true
}

function closeBookingDialog(): void {
  showBookingDialog.value = false
  editingBooking.value = null
}

async function saveBooking(input: CreateBookingInput): Promise<void> {
  dialogError.value = ''
  try {
    if (editingBooking.value) {
      await store.updateBooking(editingBooking.value.id, input)
      showNotice('success', 'Your booking was updated.')
    } else {
      await store.createBooking(input)
      showNotice('success', 'Your booking was created.')
    }

    selectedDate.value = input.date
    closeBookingDialog()
  } catch (error) {
    dialogError.value = getErrorMessage(error)
    showNotice('error', dialogError.value)
  }
}

async function cancelBooking(booking: Booking): Promise<void> {
  if (!window.confirm(`Cancel "${booking.title}"?`)) {
    return
  }

  try {
    await store.cancelBooking(booking.id)
    showNotice('success', 'Your booking was cancelled.')
  } catch (error) {
    showNotice('error', getErrorMessage(error))
  }
}

function navigateTo(item: string): void {
  activeNav.value = item
  const targetId = item === 'Bookings' ? 'bookings-section' : 'rooms-section'
  if (item === 'Overview') {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } else {
    document.getElementById(targetId)?.scrollIntoView({ behavior: 'smooth' })
  }
}

async function retryLoading(): Promise<void> {
  loadError.value = ''
  try {
    await store.load()
  } catch (error) {
    loadError.value = getErrorMessage(error)
  }
}
</script>

<template>
  <div class="app-shell">
    <aside class="sidebar">
      <a class="brand" href="#" aria-label="Gather home" @click.prevent="navigateTo('Overview')">
        <span class="brand-mark" aria-hidden="true">
          <svg viewBox="0 0 32 32" fill="none">
            <path d="M5 8.5 16 3l11 5.5v15L16 29 5 23.5v-15Z" />
            <path d="m5.5 8.5 10.5 5.4 10.5-5.4M16 14v14" />
          </svg>
        </span>
        <span>gather<span class="brand-period">.</span></span>
      </a>

      <div class="workspace-label">WORKSPACE</div>
      <nav class="primary-nav" aria-label="Main navigation">
        <button
          v-for="item in ['Overview', 'Bookings', 'Meeting rooms']"
          :key="item"
          class="nav-item"
          :class="{ 'nav-item-active': activeNav === item }"
          type="button"
          @click="navigateTo(item)"
        >
          <svg v-if="item === 'Overview'" viewBox="0 0 24 24" aria-hidden="true">
            <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" />
            <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" />
            <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" />
            <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" />
          </svg>
          <svg v-else-if="item === 'Bookings'" viewBox="0 0 24 24" aria-hidden="true">
            <rect x="4" y="5.5" width="16" height="15" rx="2" />
            <path d="M8 3.5v4M16 3.5v4M4 10h16M8 14h3M8 17h6" />
          </svg>
          <svg v-else viewBox="0 0 24 24" aria-hidden="true">
            <path d="M3.5 20.5V9l8.5-5 8.5 5v11.5h-17Z" />
            <path d="M8 20.5v-7h8v7M8 9.5h.01M12 9.5h.01M16 9.5h.01" />
          </svg>
          {{ item }}
          <span v-if="item === 'Bookings'" class="nav-count">{{ bookings.length }}</span>
        </button>
      </nav>

      <div class="sidebar-bottom">
        <div class="help-card">
          <span class="help-icon" aria-hidden="true">?</span>
          <div>
            <strong>Need a hand?</strong>
            <p>We're here to help.</p>
            <a href="mailto:workplace@example.com">Contact workplace team <span aria-hidden="true">↗</span></a>
          </div>
        </div>
        <div class="profile-button">
          <span class="avatar">MR</span>
          <span class="profile-details"><strong>Meeting rooms</strong><small>Office schedule</small></span>
        </div>
      </div>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <div class="breadcrumbs"><span>Workspace</span><span class="crumb-divider">/</span> {{ activeNav }}</div>
        <div class="topbar-actions">
          <span class="office-indicator"><span class="online-dot"></span> Office rooms</span>
        </div>
      </header>

      <div class="page-content">
        <section class="welcome-row">
          <div>
            <p class="eyebrow">YOUR WORKPLACE, MADE SIMPLE</p>
            <h1>Make room for good ideas <span aria-hidden="true">✦</span></h1>
            <p class="welcome-subtitle">Find a space and bring your people together.</p>
          </div>
          <button class="button button-primary" type="button" :disabled="!rooms.length" @click="openBookingDialog()">
            <span class="button-plus" aria-hidden="true">+</span> Book a room
          </button>
        </section>

        <Transition name="toast">
          <div
            v-if="notice"
            class="toast-notice"
            :class="`notice-${notice.kind}`"
            :role="notice.kind === 'error' ? 'alert' : 'status'"
            aria-atomic="true"
          >
            <span class="toast-icon" aria-hidden="true">{{ notice.kind === 'success' ? '✓' : '!' }}</span>
            <span>{{ notice.message }}</span>
            <button type="button" class="toast-close" aria-label="Dismiss notification" @click="dismissNotice">×</button>
          </div>
        </Transition>
        <div v-if="loadError" class="load-error" role="alert">
          <span>{{ loadError }}</span>
          <button type="button" @click="retryLoading">Try again</button>
        </div>

        <section class="hero-card" aria-label="Meeting room booking">
          <div class="hero-content">
            <span class="hero-kicker"><span class="sparkle" aria-hidden="true">✳</span> MAKE TODAY A GOOD MEETING</span>
            <h2>A little space<br />for big ideas.</h2>
            <p>Find a room that gives your team space to think.</p>
            <button class="hero-link" type="button" :disabled="!rooms.length" @click="openBookingDialog()">
              Find your room <span aria-hidden="true">→</span>
            </button>
          </div>
          <div class="hero-art" aria-hidden="true">
            <div class="art-sun"></div>
            <div class="art-window"><span></span><span></span><span></span></div>
            <div class="art-plant art-plant-one"><i></i><i></i><i></i><b></b></div>
            <div class="art-plant art-plant-two"><i></i><i></i><i></i><b></b></div>
            <div class="art-table"></div>
            <div class="art-chair"></div>
          </div>
          <div class="hero-note"><span>✦</span> Good ideas grow together</div>
        </section>

        <section class="overview-strip" aria-label="Booking overview">
          <div class="overview-date">
            <div class="calendar-icon" aria-hidden="true">
              <span>{{ new Intl.DateTimeFormat('en', { month: 'short' }).format(new Date(`${selectedDate}T12:00:00`)) }}</span>
              <strong>{{ new Date(`${selectedDate}T12:00:00`).getDate() }}</strong>
            </div>
            <div><span class="metric-label">SELECTED DAY</span><strong>{{ formattedDate }}</strong></div>
          </div>
          <div class="overview-stat">
            <span class="stat-icon stat-icon-green" aria-hidden="true">↗</span>
            <div><strong>{{ roomsWithoutBookings }}<small> / {{ rooms.length }}</small></strong><span>rooms with no bookings</span></div>
          </div>
          <div class="overview-stat">
            <span class="stat-icon stat-icon-peach" aria-hidden="true">▦</span>
            <div><strong>{{ todaysBookings.length }}</strong><span>bookings on this day</span></div>
          </div>
          <button class="overview-arrow" type="button" aria-label="Book a room" :disabled="!rooms.length" @click="openBookingDialog()">→</button>
        </section>

        <section id="rooms-section" class="rooms-section">
          <div class="section-heading">
            <div>
              <div class="section-title-row"><h2>Find your space</h2><span class="room-count">{{ filteredRooms.length }} rooms</span></div>
              <p>Choose a room with the capacity you need.</p>
            </div>
            <div class="date-control">
              <button type="button" aria-label="Previous day" @click="changeDate(-1)">‹</button>
              <label>
                <span class="sr-only">Booking date</span>
                <input v-model="selectedDate" type="date" />
              </label>
              <button type="button" aria-label="Next day" @click="changeDate(1)">›</button>
            </div>
          </div>

          <div class="room-filters">
            <label class="search-field">
              <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.8" cy="10.8" r="6.8" /><path d="m16 16 4.5 4.5" /></svg>
              <input v-model="searchQuery" type="search" placeholder="Search rooms" />
            </label>
            <label class="capacity-select">
              <span class="sr-only">Minimum room capacity</span>
              <select v-model="capacityFilter">
                <option value="any">Any capacity</option>
                <option value="4">4+ people</option>
                <option value="8">8+ people</option>
                <option value="12">12+ people</option>
              </select>
            </label>
          </div>

          <div v-if="isLoading" class="schedule-empty" aria-live="polite">Loading meeting rooms…</div>
          <div v-else-if="filteredRooms.length" class="room-grid">
            <article v-for="(room, index) in filteredRooms" :key="room.id" class="room-card">
              <div class="room-image" :class="`room-image-${index % 4}`">
                <div class="room-image-label"><span class="room-live-dot"></span> Meeting room</div>
                <div class="room-art-shape room-art-shape-one"></div>
                <div class="room-art-shape room-art-shape-two"></div>
                <div class="room-art-window"></div>
                <div class="room-art-furniture"></div>
                <div class="room-image-footer">
                  <span class="capacity-pill"><svg viewBox="0 0 20 20" aria-hidden="true"><circle cx="7" cy="6" r="2.5"/><path d="M2.5 15c.2-2.5 1.8-4 4.5-4s4.3 1.5 4.5 4M13 4a2.3 2 0 0 1 0 4.5M14 11c2.1.3 3.2 1.6 3.5 3.5"/></svg>{{ room.capacity }} people</span>
                  <span class="image-booking-count">{{ roomBookingCount(room.id) }} booked</span>
                </div>
              </div>
              <div class="room-card-body">
                <div class="room-card-title"><h3>{{ room.name }}</h3><span class="room-status"><span></span> Bookable</span></div>
                <p class="room-description">A meeting space for up to {{ room.capacity }} people.</p>
                <div class="amenity-list"><span>Capacity {{ room.capacity }}</span></div>
                <button class="room-book-button" type="button" @click="openBookingDialog(room.id)">
                  Book this room <span aria-hidden="true">→</span>
                </button>
              </div>
            </article>
          </div>
          <div v-else class="empty-state">
            <span aria-hidden="true">⌕</span>
            <h3>No rooms found</h3>
            <p>Try a different search or lower the capacity filter.</p>
            <button type="button" @click="searchQuery = ''; capacityFilter = 'any'">Clear filters</button>
          </div>
        </section>

        <section id="bookings-section" class="schedule-section">
          <div class="section-heading schedule-heading">
            <div><h2>Room schedule</h2><p>Bookings for {{ formatShortDate(selectedDate) }}.</p></div>
          </div>
          <div v-if="todaysBookings.length" class="schedule-list">
            <article v-for="booking in todaysBookings" :key="booking.id" class="schedule-item">
              <div class="schedule-time">{{ formatTime(booking.start) }}<span>{{ formatTime(booking.end) }}</span></div>
              <div class="schedule-timeline"><span></span></div>
              <div class="schedule-details">
                <div><h3>{{ booking.title }}</h3><span class="schedule-room">{{ booking.roomName }}</span></div>
                <span class="booking-confirmed"><span></span> Confirmed</span>
                <div class="booking-actions">
                  <button class="edit-button" type="button" @click="openEditDialog(booking)">Edit</button>
                  <button class="cancel-button" type="button" @click="cancelBooking(booking)">Cancel</button>
                </div>
              </div>
            </article>
          </div>
          <div v-else class="schedule-empty">
            <span class="schedule-empty-icon" aria-hidden="true">☀</span>
            <div><strong>A clear day is a good day.</strong><p>No bookings for {{ formatShortDate(selectedDate) }} yet.</p></div>
            <button class="text-button" type="button" :disabled="!rooms.length" @click="openBookingDialog()">Plan a meeting <span aria-hidden="true">→</span></button>
          </div>
        </section>
        <footer class="page-footer"><span>Made for better workdays.</span><span>Need support? <a href="mailto:workplace@example.com">Get in touch</a></span></footer>
      </div>
    </main>

    <BookingDialog
      v-if="showBookingDialog"
      :rooms="rooms"
      :initial-date="selectedDate"
      :initial-room-id="bookingRoomId"
      :booking="editingBooking"
      :submission-error="dialogError"
      @close="closeBookingDialog"
      @book="saveBooking"
    />
  </div>
</template>
