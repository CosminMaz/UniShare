import PropTypes from 'prop-types'
import { MessageBanner } from './MessageBanner'
import styles from '../Dashboard.module.css'

export function DeleteModal({
  isOpen,
  itemTitle,
  error,
  onCancel,
  onConfirm,
}) {
  if (!isOpen) return null

  return (
    <div className={styles.modalOverlay}>
      <dialog
        className={styles.modalContent}
        aria-modal="true"
        aria-labelledby="delete-modal-title"
        aria-describedby="delete-modal-description"
        tabIndex={-1}
        role="dialog"
        open
      >
        <div className={styles.modalHeader}>
          <h3 id="delete-modal-title" className={styles.modalTitle}>
            Delete item
          </h3>
          <button className={styles.modalCloseBtn} onClick={onCancel}>
            ×
          </button>
        </div>
        <div className={styles.modalBody}>
          <p id="delete-modal-description">
            Are you sure you want to delete "{itemTitle || 'this item'}"?
          </p>
          <MessageBanner type="error">{error}</MessageBanner>
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.modalCancelBtn} onClick={onCancel}>
            Cancel
          </button>
          <button className={styles.modalSubmitBtn} onClick={onConfirm}>
            Delete
          </button>
        </div>
      </dialog>
    </div>
  )
}

DeleteModal.propTypes = {
  isOpen: PropTypes.bool,
  itemTitle: PropTypes.string,
  error: PropTypes.node,
  onCancel: PropTypes.func,
  onConfirm: PropTypes.func,
}
