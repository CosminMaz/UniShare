import PropTypes from 'prop-types'
import { MessageBanner } from './MessageBanner'
import styles from '../Dashboard.module.css'

export function ReviewModal({
  isOpen,
  itemTitle,
  rating,
  comment,
  error,
  message,
  onRatingChange,
  onCommentChange,
  onClose,
  onSubmit,
}) {
  if (!isOpen) return null

  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h3 className={styles.modalTitle}>
            Write a Review for {itemTitle || 'Item'}
          </h3>
          <button className={styles.modalCloseBtn} onClick={onClose}>
            ×
          </button>
        </div>
        <div className={styles.modalBody}>
          <MessageBanner type="error">{error}</MessageBanner>
          <MessageBanner type="success">{message}</MessageBanner>

          <div className={styles.formGroup}>
            <label className={styles.modalLabel}>Rating</label>
            <input
              type="number"
              min="1"
              max="5"
              value={rating}
              onChange={(e) => onRatingChange?.(Number.parseInt(e.target.value, 10))}
              className={styles.modalInput}
              required
            />
          </div>
          <div className={styles.formGroup}>
            <label htmlFor="reviewComment" className={styles.modalLabel}>
              Comment
            </label>
            <textarea
              id="reviewComment"
              rows="4"
              value={comment}
              onChange={(e) => onCommentChange?.(e.target.value)}
              className={styles.modalTextarea}
            />
          </div>
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.modalCancelBtn} onClick={onClose}>
            Cancel
          </button>
          <button
            className={styles.modalSubmitBtn}
            onClick={onSubmit}
            disabled={rating === 0}
          >
            Submit Review
          </button>
        </div>
      </div>
    </div>
  )
}

ReviewModal.propTypes = {
  isOpen: PropTypes.bool,
  itemTitle: PropTypes.string,
  rating: PropTypes.number,
  comment: PropTypes.string,
  error: PropTypes.node,
  message: PropTypes.node,
  onRatingChange: PropTypes.func,
  onCommentChange: PropTypes.func,
  onClose: PropTypes.func,
  onSubmit: PropTypes.func,
}
