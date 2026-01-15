import { BookingCard } from './BookingCard'
import { getBookingStatus, statusColors } from '../utils'
import styles from '../Dashboard.module.css'

export function BookingsSection({
  bookings,
  items,
  isLoading = false,
  hasSubmittedReview,
  onWriteReview,
}) {
  let content

  if (isLoading) {
    content = (
      <div className={styles.loadingMessage}>Loading bookings...</div>
    )
  } else if (bookings.length === 0) {
    content = (
      <div className={styles.emptyMessage}>
        You don't have any bookings yet. Borrow an item to get started!
      </div>
    )
  } else {
    content = (
      <div className={styles.bookingsList}>
        {bookings.map((booking) => {
          const itemId = booking.ItemId ?? booking.itemId
          const bookedItem = items.find(
            (item) => item.Id === itemId || item.id === itemId,
          )
          const status = getBookingStatus(booking)
          const statusColor = statusColors[status] || '#6c757d'
          const bookingId = booking.Id || booking.id

          return (
            <BookingCard
              key={bookingId}
              booking={booking}
              itemTitle={bookedItem?.Title || bookedItem?.title}
              status={status}
              statusColor={statusColor}
              showReviewButton={
                status === 'Completed' && !hasSubmittedReview?.(bookingId)
              }
              onReview={onWriteReview}
            />
          )
        })}
      </div>
    )
  }

  return (
    <section className={`${styles.sectionCard} ${styles.bookingsSection}`}>
      <h3 className={styles.sectionTitle}>My Bookings</h3>
      <p className={styles.sectionSubtitle}>
        Track the items you are borrowing from others
      </p>
      {content}
    </section>
  )
}
