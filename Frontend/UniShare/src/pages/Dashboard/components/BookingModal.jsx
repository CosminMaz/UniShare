import styles from '../Dashboard.module.css'

export function BookingModal({
  isOpen,
  selectedItem,
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  onClose,
  onSubmit,
  isBooking = false,
  error = '',
}) {
  if (!isOpen) return null

  const dailyRate = selectedItem?.DailyRate ?? selectedItem?.dailyRate
  const estimatedTotal =
    dailyRate && startDate && endDate
      ? (
          dailyRate *
          Math.max(
            1,
            Math.ceil(
              (new Date(endDate) - new Date(startDate)) / (1000 * 60 * 60 * 24),
            ),
          )
        ).toFixed(2)
      : null

  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h3 className={styles.modalTitle}>
            Book {selectedItem?.Title || 'Item'}
          </h3>
          <button className={styles.modalCloseBtn} onClick={onClose}>
            ×
          </button>
        </div>

        <div className={styles.modalBody}>
          {error && (
            <div className={styles.errorMessage} role="alert">
              {error}
            </div>
          )}

          <div className={styles.formGroup}>
            <label htmlFor="startDate" className={styles.modalLabel}>
              Start Date
            </label>
            <input
              type="date"
              id="startDate"
              value={startDate}
              onChange={(e) => onStartDateChange?.(e.target.value)}
              className={styles.modalInput}
              min={new Date().toISOString().slice(0, 10)}
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="endDate" className={styles.modalLabel}>
              End Date
            </label>
            <input
              type="date"
              id="endDate"
              value={endDate}
              onChange={(e) => onEndDateChange?.(e.target.value)}
              className={styles.modalInput}
              min={startDate || new Date().toISOString().slice(0, 10)}
              required
            />
          </div>

          {estimatedTotal && (
            <div className={styles.priceInfo}>
              <span className={styles.priceLabel}>Estimated Total:</span>
              <span className={styles.priceValue}>${estimatedTotal}</span>
            </div>
          )}
        </div>

        <div className={styles.modalFooter}>
          <button
            className={styles.modalCancelBtn}
            onClick={onClose}
            disabled={isBooking}
          >
            Cancel
          </button>
          <button
            className={styles.modalSubmitBtn}
            onClick={onSubmit}
            disabled={isBooking || !startDate || !endDate}
          >
            {isBooking ? 'Processing...' : 'Confirm Booking'}
          </button>
        </div>
      </div>
    </div>
  )
}
