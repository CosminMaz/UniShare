import PropTypes from 'prop-types'
import { formatDailyRate } from '../utils'
import styles from '../Dashboard.module.css'

const fallbackImage = (event) => {
  event.target.src = 'https://via.placeholder.com/200?text=No+Image'
}

export function ItemCard({
  item,
  isOwner = false,
  onBorrow,
  onDelete,
  isBooking = false,
  showAvailability = false,
}) {
  const imageUrl = item.imageUrl || item.ImageUrl
  const createdAt = item.CreatedAt || item.createdAt
  const isAvailable = item.IsAvailable ?? item.isAvailable ?? true

  return (
    <div className={styles.itemCard}>
      {imageUrl && (
        <div className={styles.itemImage}>
          <img src={imageUrl} alt={item.Title} onError={fallbackImage} />
        </div>
      )}

      <div className={styles.itemHeader}>
        <h4 className={styles.itemTitle}>{item.Title || 'No title'}</h4>
        <span className={styles.itemBadge}>{item.Categ || 'General'}</span>
      </div>

      <p className={styles.itemDescription}>
        {item.Description || 'No description'}
      </p>

      <div className={styles.itemDetails}>
        <div className={styles.detailRow}>
          <span className={styles.detailLabel}>Condition:</span>
          <span className={styles.detailValue}>{item.Cond || 'N/A'}</span>
        </div>
        <div className={styles.detailRow}>
          <span className={styles.detailLabel}>Price/day:</span>
          <span className={styles.detailValue}>
            {formatDailyRate(item.DailyRate ?? item.dailyRate)}
          </span>
        </div>
        <div className={styles.detailRow}>
          <span className={styles.detailLabel}>Added:</span>
          <span className={styles.detailValue}>
            {createdAt
              ? new Date(createdAt).toLocaleDateString('en-US')
              : 'N/A'}
          </span>
        </div>
        {showAvailability && (
          <div className={styles.detailRow}>
            <span className={styles.detailLabel}>Status:</span>
            <span className={styles.detailValue}>
              <span
                className={`${styles.availabilityPill} ${
                  isAvailable
                    ? styles.availabilityAvailable
                    : styles.availabilityUnavailable
                }`}
              >
                {isAvailable ? 'Available' : 'Booked'}
              </span>
            </span>
          </div>
        )}
      </div>

      <div className={styles.itemFooter}>
        {isOwner ? (
          <button
            className={styles.deleteBtn}
            onClick={() => onDelete?.(item)}
          >
            Delete
          </button>
        ) : (
          <button
            className={styles.borrowBtn}
            onClick={() => onBorrow?.(item)}
            disabled={isBooking}
          >
            Borrow
          </button>
        )}
      </div>
    </div>
  )
}

ItemCard.propTypes = {
  item: PropTypes.shape({
    Id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    Title: PropTypes.string,
    Description: PropTypes.string,
    imageUrl: PropTypes.string,
    ImageUrl: PropTypes.string,
    Categ: PropTypes.string,
    Cond: PropTypes.string,
    DailyRate: PropTypes.number,
    dailyRate: PropTypes.number,
    CreatedAt: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    createdAt: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    OwnerId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    ownerId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
    IsAvailable: PropTypes.bool,
    isAvailable: PropTypes.bool,
  }).isRequired,
  isOwner: PropTypes.bool,
  onBorrow: PropTypes.func,
  onDelete: PropTypes.func,
  isBooking: PropTypes.bool,
  showAvailability: PropTypes.bool,
}
