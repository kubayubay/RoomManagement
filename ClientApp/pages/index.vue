<template>
    <div>
        <div class="text-2xl font-bold mb-4">
            Room Management Calendar
        </div>
        <div class="flex space-x-4 mb-4">
            <Button icon="ooui:arrow-previous-ltr" label="Previous Month" @click="onClickPreviousMonth" />
            <Button icon="ooui:arrow-previous-rtl" label="Next Month" @click="onClickNextMonth" />
        </div>
        <DayPilotMonth :config="config" ref="monthRef" />
    </div>
    <div v-if="events != undefined">
        <div v-for="event in events">
            {{ event.name }}
        </div>
    </div>
    <div v-if="rooms != undefined">
        <div v-for="room in rooms">
            {{ room.name }}
        </div>
    </div>
</template>

<script setup>
import { DayPilot, DayPilotMonth } from '@daypilot/daypilot-lite-vue'
import { ref, onMounted } from 'vue'

const currentDate = ref(new Date())

const config = ref({
    locale: 'en-us',
    timeRangeSelectedHandling: 'Enabled',
    startDate: `${currentDate.value.getFullYear()}-${String(currentDate.value.getMonth() + 1).padStart(2, '0')}-${String(currentDate.value.getDate()).padStart(2, '0')}`,
    onTimeRangeSelected: async (args) => {
        const calendar = args.control
        calendar.clearSelection()
        navigateTo(`/Day?date=${args.start.toDateLocal().toLocaleDateString()}`)
    },
    /* eventDeleteHandling: 'Update',
    onEventDeleted: (args) => {
        console.log('Event deleted: ' + args.e.text())
    },
    eventMoveHandling: 'Update',
    onEventMoved: (args) => {
        console.log('Event moved: ' + args.e.text())
    },
    eventResizeHandling: 'Update',
    onEventResized: (args) => {
        console.log('Event resized: ' + args.e.text())
    },
    eventClickHandling: 'Disabled',
    eventRightClickHandling: 'ContextMenu',
    contextMenu: new DayPilot.Menu({
        items: [
            {
                text: 'Delete',
                onClick: (args) => {
                    const dp = args.source.calendar
                    dp.events.remove(args.source)
                }
            }
        ]
    }), */
})

const monthRef = ref(null)

const loadEvents = () => {
    const events = [
        {
            id: 1,
            start: DayPilot.Date.today(),
            end: DayPilot.Date.today().addDays(1),
            text: 'Event 1'
        }
    ]

    config.value.events = events
}

const events = ref()
const loadEvents2 = () => {
    $fetch(`/api/v1/Event/Events?start=2025-06-19&end=2025-06-21`, {
        server: false,
        onResponse({ response }) {
            if (!response.ok) {
                alert(response._data)
            } else {
                events.value = response._data
            }
        }
    })
}

const rooms = ref()
const loadRooms2 = () => {
    $fetch(`/api/v1/Room/Rooms`, {
        server: false,
        onResponse({ response }) {
            if (!response.ok) {
                alert(response._data)
            } else {
                rooms.value = response._data
            }
        }
    })
}

const onClickPreviousMonth = () => {
    currentDate.value.setMonth(currentDate.value.getMonth() - 1)
    config.value.startDate = `${currentDate.value.getFullYear()}-${String(currentDate.value.getMonth() + 1).padStart(2, '0')}-${String(currentDate.value.getDate()).padStart(2, '0')}`
}

const onClickNextMonth = () => {
    currentDate.value.setMonth(currentDate.value.getMonth() + 1)
    config.value.startDate = `${currentDate.value.getFullYear()}-${String(currentDate.value.getMonth() + 1).padStart(2, '0')}-${String(currentDate.value.getDate()).padStart(2, '0')}`
}

onMounted(() => {
    loadEvents()
    loadEvents2()
    loadRooms2()
})
</script>
