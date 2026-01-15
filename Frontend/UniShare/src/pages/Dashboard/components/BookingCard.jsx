import PropTypes from 'prop-types'
import styles from '../Dashboard.module.css'

const formatDate = (value) =>
  value ? new Date(value).toLocaleDateString('en-US') : 'N/A'

export function BookingCard({
  booking,
  itemTitle,
  status,
  statusColor,
  children,
  showReviewButton = false,
  onReview,
}) {
  const startDate = booking.StartDate || booking.startDate
  const endDate = booking.EndDate || booking.endDate
  const totalPrice = booking.TotalPrice ?? booking.totalPrice
  const requestedAt = booking.RequestedAt || booking.requestedAt

  return (
    <div className={styles.bookingCard}>
      <div className={styles.bookingHeader}>
        <div className={styles.bookingItemInfo}>
          <h4 className={styles.bookingItemTitle}>
            {itemTitle || 'Unknown Item'}
          </h4>
          <span
            className={styles.bookingStatus}
            style={{ backgroundColor: statusColor }}
          >
            {status}
          </span>
        </div>
      </div>

      <div className={styles.bookingDetails}>
        <div className={styles.bookingDetailRow}>
          <span className={styles.bookingDetailLabel}>Start Date:</span>
          <span className={styles.bookingDetailValue}>
            {formatDate(startDate)}
          </span>
        </div>
        <div className={styles.bookingDetailRow}>
          <span className={styles.bookingDetailLabel}>End Date:</span>
          <span className={styles.bookingDetailValue}>{formatDate(endDate)}</span>
        </div>
        {totalPrice !== undefined && (
          <div className={styles.bookingDetailRow}>
            <span className={styles.bookingDetailLabel}>Total Price:</span>
            <span className={styles.bookingDetailValue}>
              ${Number(totalPrice || 0).toFixed(2)}
            </span>
          </div>
        )}
        <div className={styles.bookingDetailRow}>
          <span className={styles.bookingDetailLabel}>Requested:</span>
          <span className={styles.bookingDetailValue}>
            {formatDate(requestedAt)}
          </span>
        </div>
      </div>

      {(showReviewButton || children) && (
        <div className={styles.bookingActions}>
          {children}
          {showReviewButton && (
            <button
              className={styles.reviewBtn}
              onClick={() => onReview?.(booking)}
            >
              Write a Review
            </button>
          )}
        </div>
      )}
    </div>
  )
}

BookingCard.propTypes = {
  booking: PropTypes.shape({
    Id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    ItemId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    itemId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    StartDate: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    startDate: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    EndDate: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    endDate: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    TotalPrice: PropTypes.number,
    totalPrice: PropTypes.number,
    RequestedAt: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    requestedAt: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  }).isRequired,
  itemTitle: PropTypes.string,
  status: PropTypes.string,
  statusColor: PropTypes.string,
  children: PropTypes.node,
  showReviewButton: PropTypes.bool,
  onReview: PropTypes.func,
}
