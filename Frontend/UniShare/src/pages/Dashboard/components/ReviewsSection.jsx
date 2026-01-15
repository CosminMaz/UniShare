import { normalizeId } from '../utils'
import styles from '../Dashboard.module.css'

export function ReviewsSection({ reviews, items }) {
  return (
    <section className={`${styles.sectionCard} ${styles.reviewsSection}`}>
      <div className={styles.sectionHeader}>
        <div>
          <h3 className={styles.sectionTitle}>Community Stories</h3>
          <p className={styles.sectionSubtitle}>
            Borrower feedback keeps UniShare trustworthy
          </p>
        </div>
      </div>

      {reviews.length === 0 ? (
        <div className={styles.emptyMessage}>
          No reviews yet. Complete a booking to be the first.
        </div>
      ) : (
        <div className={styles.reviewsGrid}>
          {reviews.slice(0, 6).map((review) => {
            const rating = review.Rating ?? review.rating ?? 0
            const reviewerName =
              review.Reviewer?.FullName ||
              review.reviewer?.fullName ||
              review.ReviewerName ||
              'Anonymous'
            const comment =
              review.Comment || review.comment || 'No comment provided.'
            const createdAt = review.CreatedAt || review.createdAt
            const itemTitle = (() => {
              const match = items.find(
                (item) =>
                  normalizeId(item.Id ?? item.id) ===
                  normalizeId(review.ItemId ?? review.itemId),
              )
              return match?.Title || match?.title || 'Shared item'
            })()

            return (
              <article
                key={review.Id || review.id}
                className={styles.reviewCard}
              >
                <header className={styles.reviewHeader}>
                  <div className={styles.reviewMeta}>
                    <span className={styles.reviewerName}>{reviewerName}</span>
                    {createdAt && (
                      <span className={styles.reviewDate}>
                        {new Date(createdAt).toLocaleDateString('en-US')}
                      </span>
                    )}
                  </div>
                  <div className={styles.ratingBadge}>
                    {Number(rating).toFixed(1)} ★
                  </div>
                </header>
                <p className={styles.reviewComment}>{comment}</p>
                <footer className={styles.reviewFooter}>
                  <div className={styles.reviewFooterContent}>
                    <span className={styles.reviewTag}>
                      {review.RevType || review.revType || 'Review'}
                    </span>
                    <span className={styles.reviewItemLabel}>
                      for {itemTitle}
                    </span>
                  </div>
                </footer>
              </article>
            )
          })}
        </div>
      )}
    </section>
  )
}
