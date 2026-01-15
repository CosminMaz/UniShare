import PropTypes from 'prop-types'
import { ItemCard } from './ItemCard'
import { MessageBanner } from './MessageBanner'
import styles from '../Dashboard.module.css'

export function MyItemsSection({
  items,
  isLoading = false,
  error = '',
  onDelete,
}) {
  let content

  if (isLoading) {
    content = (
      <div className={styles.loadingMessage}>Loading your listings...</div>
    )
  } else if (items.length === 0) {
    content = (
      <div className={styles.emptyMessage}>
        You haven't listed any items yet. Add one to start lending!
      </div>
    )
  } else {
    content = (
      <div className={styles.itemsScroller}>
        <div className={styles.itemsTrack}>
          {items.map((item) => (
            <ItemCard
              key={item.Id || item.id}
              item={item}
              isOwner
              onDelete={onDelete}
              showAvailability
            />
          ))}
        </div>
      </div>
    )
  }

  return (
    <section className={`${styles.sectionCard} ${styles.itemsSection}`}>
      <div className={styles.sectionHeader}>
        <div>
          <h3 className={styles.sectionTitle}>My Listed Items</h3>
          <p className={styles.sectionSubtitle}>
            Track the items you are sharing with the community
          </p>
        </div>
      </div>

      <MessageBanner type="error">{error}</MessageBanner>
      {content}
    </section>
  )
}

MyItemsSection.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isLoading: PropTypes.bool,
  error: PropTypes.node,
  onDelete: PropTypes.func,
}
