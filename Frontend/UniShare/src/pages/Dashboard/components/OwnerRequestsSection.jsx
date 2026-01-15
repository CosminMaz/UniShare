import { BookingCard } from './BookingCard'
import { MessageBanner } from './MessageBanner'
import { getBookingStatus, statusColors } from '../utils'
import styles from '../Dashboard.module.css'

export function OwnerRequestsSection({
  bookings,
  items,
  isLoading = false,
  onApprove,
  onReject,
  onComplete,
  error = '',
  message = '',
}) {
  let content

  if (isLoading) {
    content = (
      <div className={styles.loadingMessage}>Loading requests...</div>
    )
  } else if (bookings.length === 0) {
    content = (
      <div className={styles.emptyMessage}>
        No booking requests yet. When someone borrows your items, they'll appear
        here.
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
          const isPending = status === 'Pending'
          const canComplete = status === 'Approved' || status === 'Active'

          return (
            <BookingCard
              key={booking.Id || booking.id}
              booking={booking}
              itemTitle={bookedItem?.Title || bookedItem?.title}
              status={status}
              statusColor={statusColor}
            >
              {isPending && (
                <>
                  <button
                    className={styles.approveBtn}
                    onClick={() => onApprove?.(booking.Id || booking.id)}
                  >
                    ✓ Approve
                  </button>
                  <button
                    className={styles.rejectBtn}
                    onClick={() => onReject?.(booking.Id || booking.id)}
                  >
                    ✗ Reject
                  </button>
                </>
              )}
              {canComplete && (
                <button
                  className={styles.completeBtn}
                  onClick={() => onComplete?.(booking.Id || booking.id)}
                >
                  Mark as Returned
                </button>
              )}
            </BookingCard>
          )
        })}
      </div>
    )
  }

  return (
    <section className={`${styles.sectionCard} ${styles.bookingsSection}`}>
      <h3 className={styles.sectionTitle}>Booking Requests</h3>
      <p className={styles.sectionSubtitle}>
        Requests for items you're lending out
      </p>
      <MessageBanner type="error">{error}</MessageBanner>
      <MessageBanner type="success">{message}</MessageBanner>
      {content}
    </section>
  )
}
