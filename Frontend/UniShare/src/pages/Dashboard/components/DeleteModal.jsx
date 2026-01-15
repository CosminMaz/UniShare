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
      <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h3 className={styles.modalTitle}>Delete item</h3>
          <button className={styles.modalCloseBtn} onClick={onCancel}>
            ×
          </button>
        </div>
        <div className={styles.modalBody}>
          <p>Are you sure you want to delete "{itemTitle || 'this item'}"?</p>
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
      </div>
    </div>
  )
}
