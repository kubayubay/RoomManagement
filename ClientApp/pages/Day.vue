<template>
    <div>
        <div class="text-2xl font-bold mb-4">
            Room Management - Day View
        </div>
        <div class="mb-4">
            <Button label="Calendar" icon="ic:outline-calendar-month" @click="onClickCalendar" />
        </div>
        <DayPilotCalendar :config="config" ref="calendarRef" />
        <EventForm ref="eventFormRef" :eventInfo="event" v-if="isEventFormShown" @update="loadEvents" />
    </div>
</template>

<script setup lang="ts">
import { DayPilot, DayPilotCalendar } from '@daypilot/daypilot-lite-vue'
import { ref, onMounted } from 'vue'
const route = useRoute()
const event = ref()
const isEventFormShown = ref(false)
const eventFormRef = ref()

const scroll = () => {
    isEventFormShown.value = true
    setTimeout(() => {
        console.log(eventFormRef.value)
        eventFormRef.value.$el.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }, 200)
}

const config = ref({
    viewType: 'Resources',
    locale: 'en-us',
    headerHeight: 30,
    cellHeight: 30,
    businessBeginsHour: 6,
    businessEndsHour: 21,
    timeRangeSelectedHandling: 'Enabled',
    onTimeRangeSelected: async (args) => {
        const modal = await DayPilot.Modal.prompt('Create a new event:', 'Event 1')
        const calendar = args.control
        calendar.clearSelection()
        if (modal.canceled) { return }
        calendar.events.add({
            start: args.start,
            end: args.end,
            id: DayPilot.guid(),
            text: modal.result,
            resource: args.resource
        })
    },
    eventDeleteHandling: 'Update',
    onEventDeleted: (args) => {
        console.log('Event deleted: ' + args.e.text())
        $fetch(`/api/v1/Event?id=${args.e.id()}`, {
            server: false,
            method: 'DELETE',
            onResponse({ response }) {
                if (!response.ok) {
                    alert(`Could not delete event ${args.e.text()}.`)
                }
                loadEvents()
            }
        })
    },
    eventMoveHandling: 'Update',
    onEventMoved: (args) => {
        console.log('Event moved: ' + args.e.text())
    },
    eventResizeHandling: 'Update',
    onEventResized: (args) => {
        console.log('Event resized: ' + args.e.text())
    },
    eventClickHandling: 'ContextMenu',
    eventRightClickHandling: 'ContextMenu',
    contextMenu: new DayPilot.Menu({
        items: [
            {
                text: 'Edit', onClick: (args) => {
                    let id = args.source.id()
                    $fetch(`/api/v1/Event?id=${id}`, {
                        server: false,
                        onResponse({ response }) {
                            event.value = response._data
                            isEventFormShown.value = true
                            scroll()
                        }
                    })
                }
            },
            {
                text: 'Delete', onClick: (args) => {
                    const dp = args.source.calendar
                    dp.events.remove(args.source)
                    // console.log('Deleted through menu')
                    console.log(args)
                    $fetch(`/api/v1/Event?id=${args.source.id()}`, {
                        server: false,
                        method: 'DELETE',
                        onResponse({ response }) {
                            if (!response.ok) {
                                alert(`Could not delete event ${args.source.text()}.`)
                            }
                            loadEvents()
                        }
                    })
                }
            }
        ]
    }),
})

const calendarRef = ref(null)

const loadEvents = () => {
    let events = []
    let date = route.query.date
    isEventFormShown.value = false
    config.value.startDate = new Date(date)
    $fetch(`/api/v1/Event/Events?start=${date}&end=${date}`, {
        server: false,
        onResponse({ response }) {
            for (let event of response._data) {
                let eventStart = new Date(event.startAt)
                let eventEnd = new Date(event.endAt)
                events.push({
                    id: event.id,
                    start: eventStart.setHours(eventStart.getHours() - 7),
                    end: eventEnd.setHours(eventEnd.getHours() - 7),
                    text: event.name,
                    resource: event.roomId
                })
            }
            config.value.events = events
        }
    })
}

const loadResources = () => {
    $fetch('/api/v1/Room/Rooms', {
        server: false,
        onResponse({ response }) {
            config.value.columns = response._data
        }
    })
}

const onClickCalendar = () => navigateTo('/')

onMounted(() => {
    loadEvents()        // load events
    loadResources()     // load rooms (resources)
})
</script>
