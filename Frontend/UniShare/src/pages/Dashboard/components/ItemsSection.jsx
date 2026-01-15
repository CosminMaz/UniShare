import PropTypes from 'prop-types'
import { ItemCard } from './ItemCard'
import { MessageBanner } from './MessageBanner'
import styles from '../Dashboard.module.css'

export function ItemsSection({
  title,
  subtitle,
  items,
  isLoading = false,
  errors = [],
  successes = [],
  onBorrow,
  onDelete,
  isBooking = false,
  currentUserId,
}) {
  let content

  if (isLoading) {
    content = <div className={styles.loadingMessage}>Loading Items...</div>
  } else if (items.length === 0) {
    content = (
      <div className={styles.emptyMessage}>
        There are no items available at this time.
      </div>
    )
  } else {
    content = (
      <div className={styles.itemsGrid}>
        {items.map((item) => (
          <ItemCard
            key={item.Id || item.id}
            item={item}
            isOwner={currentUserId === (item.OwnerId ?? item.ownerId)}
            onBorrow={onBorrow}
            onDelete={onDelete}
            isBooking={isBooking}
          />
        ))}
      </div>
    )
  }

  return (
    <section className={`${styles.sectionCard} ${styles.itemsSection}`}>
      <div className={styles.sectionHeader}>
        <div>
          <h3 className={styles.sectionTitle}>{title}</h3>
          <p className={styles.sectionSubtitle}>{subtitle}</p>
        </div>
      </div>

      {errors.filter(Boolean).map((message) => (
        <MessageBanner key={`error-${message}`} type="error">
          {message}
        </MessageBanner>
      ))}

      {successes.filter(Boolean).map((message) => (
        <MessageBanner key={`success-${message}`} type="success">
          {message}
        </MessageBanner>
      ))}

      {content}
    </section>
  )
}

ItemsSection.propTypes = {
  title: PropTypes.string.isRequired,
  subtitle: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isLoading: PropTypes.bool,
  errors: PropTypes.arrayOf(PropTypes.node),
  successes: PropTypes.arrayOf(PropTypes.node),
  onBorrow: PropTypes.func,
  onDelete: PropTypes.func,
  isBooking: PropTypes.bool,
  currentUserId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
}
