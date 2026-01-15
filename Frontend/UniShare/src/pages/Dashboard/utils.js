import axios from 'axios'

export const statusColors = {
  Pending: '#ffc107',
  Approved: '#28a745',
  Active: '#17a2b8',
  Completed: '#6c757d',
  Canceled: '#dc3545',
  Rejected: '#dc3545',
}

export const getApiErrorMessage = (err, defaultMessage) => {
  if (axios.isAxiosError(err) && err.response) {
    const errorData = err.response.data
    if (errorData?.error) return errorData.error
    if (errorData?.errors) {
      return Object.entries(errorData.errors)
        .map(([key, values]) => `${key}: ${values.join(', ')}`)
        .join('; ')
    }
    if (errorData?.message) return errorData.message
    if (err.response.status === 401) {
      return 'You are not authenticated. Please log in again.'
    }
    return defaultMessage
  }
  return err.message || 'An unexpected error occurred.'
}

export const formatDailyRate = (value) => {
  const parsed = Number(value)
  if (value == null || Number.isNaN(parsed)) {
    return 'N/A'
  }
  return `$${parsed.toFixed(2)}`
}

export const getBookingStatus = (booking) =>
  booking?.Status || booking?.status || 'Pending'

export const normalizeId = (value) =>
  (typeof value === 'string' ? value : value?.toString?.()) ?? ''

export const getReviewTypeFromRating = (rating) => {
  if (rating <= 1) return 'Bad'
  if (rating === 2) return 'Ok'
  if (rating === 3) return 'Good'
  return 'VeryGood'
}
