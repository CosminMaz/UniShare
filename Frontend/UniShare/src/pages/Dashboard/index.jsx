import { useState, useEffect } from 'react'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'
import styles from './Dashboard.module.css'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

const getApiErrorMessage = (err, defaultMessage) => {
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

const statusColors = {
  Pending: '#ffc107',
  Approved: '#28a745',
  Active: '#17a2b8',
  Completed: '#6c757d',
  Canceled: '#dc3545',
  Rejected: '#dc3545',
}

export default function DashboardPage() {
  const [user, setUser] = useState(null)
  const [items, setItems] = useState([])
  const [myItems, setMyItems] = useState([])
  const [bookings, setBookings] = useState([])
  const [ownerBookings, setOwnerBookings] = useState([])
  const [reviews, setReviews] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMyItems, setIsLoadingMyItems] = useState(false)
  const [isLoadingBookings, setIsLoadingBookings] = useState(false)
  const [error, setError] = useState('')
  const [myItemsError, setMyItemsError] = useState('')
  const [deleteError, setDeleteError] = useState('')
  const [deleteMessage, setDeleteMessage] = useState('')
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [itemToDelete, setItemToDelete] = useState(null)
  const [bookingMessage, setBookingMessage] = useState('')
  const [bookingRequestError, setBookingRequestError] = useState('')
  const [bookingRequestMessage, setBookingRequestMessage] = useState('')
  const [isBooking, setIsBooking] = useState(false)
  const [showBookingModal, setShowBookingModal] = useState(false)
  const [selectedItem, setSelectedItem] = useState(null)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [showReviewModal, setShowReviewModal] = useState(false);
  const [reviewBooking, setReviewBooking] = useState(null);
  const [reviewRating, setReviewRating] = useState(0);
  const [reviewComment, setReviewComment] = useState('');
  const [reviewError, setReviewError] = useState('');
  const [reviewMessage, setReviewMessage] = useState('');

  const formatDailyRate = (value) => {
    const parsed = Number(value)
    if (value == null || Number.isNaN(parsed)) {
      return 'N/A'
    }
    return `$${parsed.toFixed(2)}`
  }

  const getBookingStatus = (booking) => booking?.Status || booking?.status || 'Pending'

  const normalizeId = (value) =>
    typeof value === 'string' ? value : value?.toString?.() ?? ''

  const hasSubmittedReview = (bookingId) => {
    const targetId = normalizeId(bookingId)
    return reviews.some(
      (review) => normalizeId(review.BookingId ?? review.bookingId) === targetId,
    )
  }

  const getReviewTypeFromRating = (rating) => {
    if (rating <= 1) return 'Bad'
    if (rating === 2) return 'Ok'
    if (rating === 3) return 'Good'
    return 'VeryGood'
  }

  useEffect(() => {
    const storedUser = localStorage.getItem('currentUser')
    if (storedUser) {
      try {
        const parsed = JSON.parse(storedUser)
        console.log("User brut din localStorage:", parsed)
        setUser(parsed)
      } catch (err) {
        console.error('Failed to parse user:', err)
      }
    }


    const shouldRefresh = localStorage.getItem('refreshItems') === 'true'
    if (shouldRefresh) {
      localStorage.removeItem('refreshItems')
    }

    fetchItems()
    fetchMyItems()
    fetchBookings()
    fetchReviews()
  }, [])

  useEffect(() => {
    console.log('Items actualizate in state:', items)
  }, [items])

  const fetchItems = async () => {
    try {
      setIsLoading(true)
      const response = await axios.get(`${API_BASE_URL}/items`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
        },
      })

      const data = response.data
      console.log('Items primite de la API:', data)
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to fetch items'))
      console.error('Error fetching items:', err)
    } finally {
      setIsLoading(false)
    }
  }

  const fetchMyItems = async () => {
    const token =
      localStorage.getItem('accessToken') ?? localStorage.getItem('token')

    if (!token) {
      setMyItems([])
      return
    }

    try {
      setIsLoadingMyItems(true)
      setMyItemsError('')
      const response = await axios.get(`${API_BASE_URL}/items/mine`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })

      const data = response.data
      setMyItems(Array.isArray(data) ? data : [])
    } catch (err) {
      console.error('Error fetching my items:', err)
      setMyItemsError(getApiErrorMessage(err, 'Failed to fetch your listings.'))
    } finally {
      setIsLoadingMyItems(false)
    }
  }

  const fetchBookings = async () => {
    try {
      setIsLoadingBookings(true)
      const token =
        localStorage.getItem('accessToken') ?? localStorage.getItem('token')

      if (!token) return

      const response = await axios.get(`${API_BASE_URL}/bookings`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })

      const allBookings = Array.isArray(response.data) ? response.data : []
      
      const storedUser = localStorage.getItem('currentUser')
      if (!storedUser) {
        setBookings(allBookings)
        setOwnerBookings([])
        return
      }

      try {
        const currentUser = JSON.parse(storedUser)
        const userId = currentUser.Id ?? currentUser.id
        const myBookings = allBookings.filter(
          (booking) => (booking.BorrowerId ?? booking.borrowerId) === userId,
        )
        const myOwnerBookings = allBookings.filter(
          (booking) => (booking.OwnerId ?? booking.ownerId) === userId,
        )
        setBookings(myBookings)
        setOwnerBookings(myOwnerBookings)
      } catch (e) {
        console.error('Failed to parse user for booking filter:', e)
        setBookings(allBookings)
        setOwnerBookings([])
      }
    } catch (err) {
      console.error('Error fetching bookings:', err)
      // Don't show error for bookings, just log it
    } finally {
      setIsLoadingBookings(false)
    }
  }

  const fetchReviews = async () => {
    try {
      const token =
        localStorage.getItem('accessToken') ?? localStorage.getItem('token')

      const config = {}
      if (token) {
        config.headers = { Authorization: `Bearer ${token}` }
      }
      const response = await axios.get(`${API_BASE_URL}/reviews`, config)

      setReviews(Array.isArray(response.data) ? response.data : [])
    } catch (err) {
      console.error('Error fetching reviews:', err)
    }
  }

  useEffect(() => {
    const hubUrl = `${API_BASE_URL.replace(/\/$/, '')}/hub/notifications`

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { withCredentials: true })
      .withAutomaticReconnect()
      .build()

    const refreshItems = () => {
      fetchItems()
      fetchMyItems()
    }

    const refreshBookings = () => {
      fetchBookings()
      refreshItems()
    }

    connection.on('ItemCreated', refreshItems)
    connection.on('ItemUpdated', refreshItems)
    connection.on('BookingUpdated', refreshBookings)

    connection
      .start()
      .catch((err) => console.error('Failed to connect to realtime hub:', err))

    return () => {
      connection.off('ItemCreated', refreshItems)
      connection.off('ItemUpdated', refreshItems)
      connection.off('BookingUpdated', refreshBookings)
      connection.stop().catch(() => {})
    }
  }, [])

  const handleDeleteItem = (item) => {
    setDeleteError('')
    setDeleteMessage('')
    setItemToDelete(item)
    setShowDeleteModal(true)
  }

  const confirmDeleteItem = async () => {
    if (!itemToDelete) return
    const token =
      localStorage.getItem('accessToken') ?? localStorage.getItem('token')

    if (!token) {
      setDeleteError('You need to be logged in to delete an item.')
      return
    }

    const currentUserId = user?.Id ?? user?.id
    const ownerId = itemToDelete.OwnerId ?? itemToDelete.ownerId
    if (!currentUserId || currentUserId !== ownerId) {
      setDeleteError('You can only delete items you posted.')
      return
    }

    try {
      setDeleteError('')
      setDeleteMessage('')

      await axios.delete(`${API_BASE_URL}/items/${itemToDelete.Id ?? itemToDelete.id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })

      setDeleteMessage('Item deleted successfully.')
      setShowDeleteModal(false)
      setItemToDelete(null)
      fetchItems()
      fetchMyItems()
    } catch (err) {
      console.error('Error deleting item:', err)
      setDeleteError(getApiErrorMessage(err, 'Failed to delete the item. Please try again.'))
    }
  }

  const cancelDelete = () => {
    setShowDeleteModal(false)
    setItemToDelete(null)
    setDeleteError('')
  }

  const handleBorrowClick = (item) => {
    const token =
      localStorage.getItem('accessToken') ?? localStorage.getItem('token')

    if (!token) {
      setError('You need to be logged in to make a booking.')
      return
    }

    const today = new Date()
    const defaultStart = today.toISOString().slice(0, 10)
    const defaultEnd = new Date(
      today.getTime() + 2 * 24 * 60 * 60 * 1000,
    )
      .toISOString()
      .slice(0, 10)

    setSelectedItem(item)
    setStartDate(defaultStart)
    setEndDate(defaultEnd)
    setShowBookingModal(true)
    setError('')
    setBookingMessage('')
  }

  const validateBookingRequest = (token, startDate, endDate) => {
    if (!token) {
      return 'You need to be logged in to make a booking.'
    }
    if (!startDate || !endDate) {
      return 'Please select both start and end dates.'
    }
    const start = new Date(startDate)
    const end = new Date(endDate)
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
      return 'Invalid date format.'
    }
    if (end <= start) {
      return 'End date must be after start date.'
    }
    return null
  }

  const handleBookingSubmit = async () => {
    if (!selectedItem) return

    const token =
      localStorage.getItem('accessToken') ?? localStorage.getItem('token')

    const validationError = validateBookingRequest(token, startDate, endDate)
    if (validationError) {
      setError(validationError)
      return
    }

    try {
      setIsBooking(true)
      setError('')

      const payload = {
        ItemId: selectedItem.Id ?? selectedItem.id,
        StartDate: new Date(startDate).toISOString(),
        EndDate: new Date(endDate).toISOString(),
      }

      await axios.post(`${API_BASE_URL}/bookings`, payload, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      })

      setBookingMessage(
        'Booking request sent successfully! You can check with the owner for confirmation.',
      )
      setShowBookingModal(false)
      setSelectedItem(null)
      setStartDate('')
      setEndDate('')
      // Refresh bookings to show the new one
      fetchBookings()
    } catch (err) {
      console.error('Error creating booking:', err)
      setError(getApiErrorMessage(err, 'Failed to create booking.'))
    } finally {
      setIsBooking(false)
    }
  }

  const handleCloseModal = () => {
    setShowBookingModal(false)
    setSelectedItem(null)
    setStartDate('')
    setEndDate('')
    setError('')
  }

  const handleApproveBooking = async (bookingId) => {
    try {
      setBookingRequestError('')
      setBookingRequestMessage('')
      
      const token =
        localStorage.getItem('accessToken') ?? localStorage.getItem('token')

      if (!token) {
        setBookingRequestError('You need to be logged in.')
        return
      }

      await axios.post(
        `${API_BASE_URL}/bookings/${bookingId}/approve`,
        { Approve: true },
        {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        },
      )

      setBookingRequestMessage('Booking approved successfully!')
      fetchBookings()
      fetchItems() // Refresh items to update availability
      fetchMyItems()
      
      // Clear message after 3 seconds
      setTimeout(() => setBookingRequestMessage(''), 3000)
    } catch (err) {
      console.error('Error approving booking:', err)
      setBookingRequestError(getApiErrorMessage(err, 'Failed to approve booking.'))
    }
  }

  const handleRejectBooking = async (bookingId) => {
    try {
      setBookingRequestError('')
      setBookingRequestMessage('')
      
      const token =
        localStorage.getItem('accessToken') ?? localStorage.getItem('token')

      if (!token) {
        setBookingRequestError('You need to be logged in.')
        return
      }

      await axios.post(
        `${API_BASE_URL}/bookings/${bookingId}/reject`,
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        },
      )

      setBookingRequestMessage('Booking rejected.')
      fetchBookings()
      
      // Clear message after 3 seconds
      setTimeout(() => setBookingRequestMessage(''), 3000)
    } catch (err) {
      console.error('Error rejecting booking:', err)
      setBookingRequestError(getApiErrorMessage(err, 'Failed to reject booking.'))
    }
  }

  const handleCompleteBooking = async (bookingId) => {
    try {
      setBookingRequestError('');
      setBookingRequestMessage('');

      const token = localStorage.getItem('accessToken') ?? localStorage.getItem('token');
      if (!token) {
        setBookingRequestError('You need to be logged in.');
        return;
      }

      await axios.post(
        `${API_BASE_URL}/bookings/${bookingId}/complete`,
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      setBookingRequestMessage('Booking marked as returned!');
      fetchBookings(); // Refresh the bookings list
      fetchItems(); // Refresh availability
      fetchMyItems();

      setTimeout(() => setBookingRequestMessage(''), 3000);
    } catch (err) {
      console.error('Error completing booking:', err);
      setBookingRequestError(getApiErrorMessage(err, 'Failed to mark booking as returned.'))
    }
  };

  const handleWriteReviewClick = (booking) => {
    setReviewBooking(booking);
    setShowReviewModal(true);
    setReviewError('');
    setReviewMessage('');
    setReviewRating(0);
    setReviewComment('');
  };

  const handleCloseReviewModal = () => {
    setShowReviewModal(false);
    setReviewBooking(null);
  };

  const handleReviewSubmit = async () => {
    if (!reviewBooking || reviewRating === 0) {
      setReviewError('Please provide a rating.');
      return;
    }

    const token = localStorage.getItem('accessToken') ?? localStorage.getItem('token');
    if (!token) {
      setReviewError('You need to be logged in to leave a review.');
      return;
    }

    const reviewerId = user?.Id ?? user?.id
    if (!reviewerId) {
      setReviewError('We could not determine your user account. Please log in again.')
      return
    }

    const bookingId = reviewBooking.Id || reviewBooking.id
    const itemId = reviewBooking.ItemId || reviewBooking.itemId
    if (!itemId) {
      setReviewError('We could not determine which item to review. Please refresh and try again.')
      return
    }

    try {
      const payload = {
        BookingId: bookingId,
        ReviewerId: reviewerId,
        ItemId: itemId,
        Rating: reviewRating,
        Comment: reviewComment,
        RevType: getReviewTypeFromRating(reviewRating),
      };

      await axios.post(`${API_BASE_URL}/reviews`, payload, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      setReviewMessage('Thank you for your review!');
      fetchReviews();
      setTimeout(() => {
        handleCloseReviewModal();
      }, 2000);

    } catch (err) {
      console.error('Error submitting review:', err);
      setReviewError(getApiErrorMessage(err, 'Failed to submit review.'))
    }
  };

  const handleNavigateToAddItem = () => {
    globalThis.location.href = '/add-item'
  }

  const formatStatValue = (value) =>
    Number(value ?? 0).toLocaleString('en-US')

  const activeBorrowings = bookings.filter((booking) => {
    const status = getBookingStatus(booking)
    return status === 'Approved' || status === 'Active'
  }).length

  const pendingOwnerRequests = ownerBookings.filter(
    (booking) => getBookingStatus(booking) === 'Pending',
  ).length

  const dashboardStats = [
    { label: 'Marketplace Items', value: items.length, hint: 'Live listings' },
    { label: 'My Listings', value: myItems.length, hint: 'Shared with students' },
    { label: 'Active Borrowings', value: activeBorrowings, hint: 'In progress' },
    { label: 'Pending Requests', value: pendingOwnerRequests, hint: 'Need review' },
  ]

  const handleLogout = () => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('currentUser')
    globalThis.location.href = '/'
  }

  let itemsContent
  if (isLoading) {
    itemsContent = (
      <div className={styles.loadingMessage}>Loading Items...</div>
    )
  } else if (items.length === 0) {
    itemsContent = (
      <div className={styles.emptyMessage}>
        There are no items available at this time.
      </div>
    )
  } else {
    itemsContent = (
      <div className={styles.itemsGrid}>
        {items.map(item => (
          <div key={item.Id || item.id} className={styles.itemCard}>
            {(item.imageUrl || item.ImageUrl) && (
              <div className={styles.itemImage}>
                <img
                  src={item.imageUrl || item.ImageUrl}
                  alt={item.Title}
                  onError={e => {
                    e.target.src =
                      'https://via.placeholder.com/200?text=No+Image'
                  }}
                />
              </div>
            )}
            <div className={styles.itemHeader}>
              <h4 className={styles.itemTitle}>
                {item.Title || 'No title'}
              </h4>
              <span className={styles.itemBadge}>
                {item.Categ || 'General'}
              </span>
            </div>
            <p className={styles.itemDescription}>
              {item.Description || 'No description'}
            </p>
            <div className={styles.itemDetails}>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Condition:</span>
                <span className={styles.detailValue}>
                  {item.Cond || 'N/A'}
                </span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Price/day:</span>
                <span className={styles.detailValue}>
                  {item.DailyRate
                    ? `$${item.DailyRate.toFixed(2)}`
                    : 'N/A'}
                </span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Added:</span>
                <span className={styles.detailValue}>
                  {item.CreatedAt
                    ? new Date(item.CreatedAt).toLocaleDateString('en-US')
                    : 'N/A'}
                </span>
              </div>
            </div>
            <div className={styles.itemFooter}>
              {user && (user.Id ?? user.id) === (item.OwnerId ?? item.ownerId) ? (
                <button
                  className={styles.deleteBtn}
                  onClick={() => handleDeleteItem(item)}
                >
                  Delete
                </button>
              ) : (
                <button
                  className={styles.borrowBtn}
                  onClick={() => handleBorrowClick(item)}
                  disabled={isBooking}
                >
                  Borrow
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    )
  }

  let myItemsContent
  if (isLoadingMyItems) {
    myItemsContent = (
      <div className={styles.loadingMessage}>Loading your listings...</div>
    )
  } else if (myItems.length === 0) {
    myItemsContent = (
      <div className={styles.emptyMessage}>
        You haven't listed any items yet. Add one to start lending!
      </div>
    )
  } else {
    myItemsContent = (
      <div className={styles.itemsScroller}>
        <div className={styles.itemsTrack}>
          {myItems.map((item) => {
            const isAvailable = item.IsAvailable ?? item.isAvailable ?? true
            return (
              <div key={item.Id || item.id} className={styles.itemCard}>
                {(item.imageUrl || item.ImageUrl) && (
                  <div className={styles.itemImage}>
                    <img
                      src={item.imageUrl || item.ImageUrl}
                      alt={item.Title}
                      onError={e => {
                        e.target.src =
                          'https://via.placeholder.com/200?text=No+Image'
                      }}
                    />
                  </div>
                )}
                <div className={styles.itemHeader}>
                  <h4 className={styles.itemTitle}>
                    {item.Title || 'No title'}
                  </h4>
                  <span className={styles.itemBadge}>
                    {item.Categ || 'General'}
                  </span>
                </div>
                <p className={styles.itemDescription}>
                  {item.Description || 'No description'}
                </p>
                <div className={styles.itemDetails}>
                  <div className={styles.detailRow}>
                    <span className={styles.detailLabel}>Condition:</span>
                    <span className={styles.detailValue}>
                      {item.Cond || 'N/A'}
                    </span>
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
                      {item.CreatedAt
                        ? new Date(item.CreatedAt).toLocaleDateString('en-US')
                        : 'N/A'}
                    </span>
                  </div>
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
                </div>
                <div className={styles.itemFooter}>
                  <button
                    className={styles.deleteBtn}
                    onClick={() => handleDeleteItem(item)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            )
          })}
        </div>
      </div>
    )
  }

  let myBookingsContent
  if (isLoadingBookings) {
    myBookingsContent = (
      <div className={styles.loadingMessage}>Loading bookings...</div>
    )
  } else if (bookings.length === 0) {
    myBookingsContent = (
      <div className={styles.emptyMessage}>
        You don't have any bookings yet. Borrow an item to get started!
      </div>
    )
  } else {
    myBookingsContent = (
      <div className={styles.bookingsList}>
        {bookings.map((booking) => {
          // Find the item for this booking
          const bookedItem = items.find(
            (item) =>
              item.Id === booking.ItemId || item.id === booking.ItemId,
          )
          const status = getBookingStatus(booking)
          const statusColor = statusColors[status] || '#6c757d'

          return (
            <div key={booking.Id || booking.id} className={styles.bookingCard}>
              <div className={styles.bookingHeader}>
                <div className={styles.bookingItemInfo}>
                  <h4 className={styles.bookingItemTitle}>
                    {bookedItem?.Title || bookedItem?.title || 'Unknown Item'}
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
                    {booking.StartDate || booking.startDate
                      ? new Date(
                          booking.StartDate || booking.startDate,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
                <div className={styles.bookingDetailRow}>
                  <span className={styles.bookingDetailLabel}>End Date:</span>
                  <span className={styles.bookingDetailValue}>
                    {booking.EndDate || booking.endDate
                      ? new Date(
                          booking.EndDate || booking.endDate,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
                {booking.TotalPrice !== undefined ||
                booking.totalPrice !== undefined ? (
                  <div className={styles.bookingDetailRow}>
                    <span className={styles.bookingDetailLabel}>Total Price:</span>
                    <span className={styles.bookingDetailValue}>
                      $
                      {(
                        booking.TotalPrice || booking.totalPrice || 0
                      ).toFixed(2)}
                    </span>
                  </div>
                ) : null}
                <div className={styles.bookingDetailRow}>
                  <span className={styles.bookingDetailLabel}>Requested:</span>
                  <span className={styles.bookingDetailValue}>
                    {booking.RequestedAt || booking.requestedAt
                      ? new Date(
                          booking.RequestedAt || booking.requestedAt,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
              </div>
              {status === 'Completed' && !hasSubmittedReview(booking.Id || booking.id) && (
                <div className={styles.bookingActions}>
                  <button
                    className={styles.reviewBtn}
                    onClick={() => handleWriteReviewClick(booking)}
                  >
                    Write a Review
                  </button>
                </div>
              )}
            </div>
          )
        })}
      </div>
    )
  }

  let ownerBookingsContent
  if (isLoadingBookings) {
    ownerBookingsContent = (
      <div className={styles.loadingMessage}>Loading requests...</div>
    )
  } else if (ownerBookings.length === 0) {
    ownerBookingsContent = (
      <div className={styles.emptyMessage}>
        No booking requests yet. When someone borrows your items, they'll appear here.
      </div>
    )
  } else {
    ownerBookingsContent = (
      <div className={styles.bookingsList}>
        {ownerBookings.map((booking) => {
          // Find the item for this booking
          const bookedItem = items.find(
            (item) =>
              item.Id === booking.ItemId || item.id === booking.ItemId,
          )

          const status = getBookingStatus(booking);
          const statusColor = statusColors[status] || '#6c757d';
          const isPending = status === 'Pending'
          const canComplete = status === 'Approved' || status === 'Active';

          return (
            <div key={booking.Id || booking.id} className={styles.bookingCard}>
              <div className={styles.bookingHeader}>
                <div className={styles.bookingItemInfo}>
                  <h4 className={styles.bookingItemTitle}>
                    {bookedItem?.Title || bookedItem?.title || 'Unknown Item'}
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
                    {booking.StartDate || booking.startDate
                      ? new Date(
                          booking.StartDate || booking.startDate,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
                <div className={styles.bookingDetailRow}>
                  <span className={styles.bookingDetailLabel}>End Date:</span>
                  <span className={styles.bookingDetailValue}>
                    {booking.EndDate || booking.endDate
                      ? new Date(
                          booking.EndDate || booking.endDate,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
                {booking.TotalPrice !== undefined ||
                booking.totalPrice !== undefined ? (
                  <div className={styles.bookingDetailRow}>
                    <span className={styles.bookingDetailLabel}>Total Price:</span>
                    <span className={styles.bookingDetailValue}>
                      $
                      {(
                        booking.TotalPrice || booking.totalPrice || 0
                      ).toFixed(2)}
                    </span>
                  </div>
                ) : null}
                <div className={styles.bookingDetailRow}>
                  <span className={styles.bookingDetailLabel}>Requested:</span>
                  <span className={styles.bookingDetailValue}>
                    {booking.RequestedAt || booking.requestedAt
                      ? new Date(
                          booking.RequestedAt || booking.requestedAt,
                        ).toLocaleDateString('en-US')
                      : 'N/A'}
                  </span>
                </div>
              </div>
              <div className={styles.bookingActions}>
                  {isPending && (
                    <>
                      <button
                        className={styles.approveBtn}
                        onClick={() =>
                          handleApproveBooking(booking.Id || booking.id)
                        }
                      >
                        ✓ Approve
                      </button>
                      <button
                        className={styles.rejectBtn}
                        onClick={() =>
                          handleRejectBooking(booking.Id || booking.id)
                        }
                      >
                        ✗ Reject
                      </button>
                    </>
                  )}
                  {canComplete && (
                    <button
                      className={styles.completeBtn}
                      onClick={() =>
                        handleCompleteBooking(booking.Id || booking.id)
                      }
                    >
                      Mark as Returned
                    </button>
                  )}
              </div>
            </div>
          )
        })}
      </div>
    )
  }

  return (
    <div className={styles.container}>
      <nav className={styles.navbar}>
        <div className={styles.navContent}>
          <h1 className={styles.title}>UniShare</h1>
          <button className={styles.logoutBtn} onClick={handleLogout}>
            Disconnect
          </button>
        </div>
      </nav>

      <main className={styles.main}>
        <section className={styles.hero}>
          <div className={styles.heroContent}>
            <p className={styles.heroEyebrow}>Dashboard</p>
            <h2 className={styles.heroTitle}>
              Welcome, {user?.FullName || user?.Email || 'UniShare member'} 👋
            </h2>
            <p className={styles.heroSubtitle}>
              Keep lending momentum going—review requests, connect with borrowers,
              and showcase the items that make life easier for fellow students.
            </p>
            <div className={styles.heroActions}>
              <button className={styles.primaryBtn} onClick={handleNavigateToAddItem}>
                List a New Item
              </button>
            </div>
          </div>
          <div className={styles.heroStats}>
            {dashboardStats.map((stat) => (
              <div key={stat.label} className={styles.statCard}>
                <span className={styles.statLabel}>{stat.label}</span>
                <span className={styles.statValue}>{formatStatValue(stat.value)}</span>
                <span className={styles.statHint}>{stat.hint}</span>
              </div>
            ))}
          </div>
        </section>

        <div className={styles.marketplaceColumn}>
        <section className={`${styles.sectionCard} ${styles.itemsSection}`}>
          <div className={styles.sectionHeader}>
            <div>
              <h3 className={styles.sectionTitle}>Items available</h3>
              <p className={styles.sectionSubtitle}>
                Discover what the UniShare community is lending today
              </p>
            </div>
          </div>

          {error && (
            <div className={styles.errorMessage} role="alert">
              {error}
            </div>
          )}
          {deleteError && (
            <div className={styles.errorMessage} role="alert">
              {deleteError}
            </div>
          )}

          {bookingMessage && !error && (
            <output className={styles.successMessage}>
              {bookingMessage}
            </output>
          )}
          {deleteMessage && !deleteError && (
            <output className={styles.successMessage}>
              {deleteMessage}
            </output>
          )}

          {itemsContent}
        </section>

        <section className={`${styles.sectionCard} ${styles.itemsSection}`}>
          <div className={styles.sectionHeader}>
            <div>
              <h3 className={styles.sectionTitle}>My Listed Items</h3>
              <p className={styles.sectionSubtitle}>
                Track the items you are sharing with the community
              </p>
            </div>
          </div>

          {myItemsError && (
            <div className={styles.errorMessage} role="alert">
              {myItemsError}
            </div>
          )}

          {myItemsContent}
        </section>
        </div>

        {/* My Bookings Section */}
        <div className={styles.bookingsGrid}>
        <section className={`${styles.sectionCard} ${styles.bookingsSection}`}>
          <h3 className={styles.sectionTitle}>My Bookings</h3>
          <p className={styles.sectionSubtitle}>
            Track the items you are borrowing from others
          </p>

          {myBookingsContent}
        </section>

        {/* Booking Requests Section (where user is owner) */}
        <section className={`${styles.sectionCard} ${styles.bookingsSection}`}>
          <h3 className={styles.sectionTitle}>Booking Requests</h3>
          <p className={styles.sectionSubtitle}>
            Requests for items you're lending out
          </p>

          {bookingRequestError && (
            <div className={styles.errorMessage} role="alert">
              {bookingRequestError}
            </div>
          )}

          {bookingRequestMessage && !bookingRequestError && (
            <output className={styles.successMessage}>
              {bookingRequestMessage}
            </output>
          )}

          {ownerBookingsContent}
        </section>
        </div>

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
                const comment = review.Comment || review.comment || 'No comment provided.'
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
                  <article key={review.Id || review.id} className={styles.reviewCard}>
                    <header className={styles.reviewHeader}>
                      <div className={styles.reviewMeta}>
                        <span className={styles.reviewerName}>{reviewerName}</span>
                        {createdAt && (
                          <span className={styles.reviewDate}>
                            {new Date(createdAt).toLocaleDateString('en-US')}
                          </span>
                        )}
                      </div>
                      <div className={styles.ratingBadge}>{Number(rating).toFixed(1)} ★</div>
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

        {user && (
          <section className={`${styles.sectionCard} ${styles.userInfo}`}>
            <h3 className={styles.infoTitle}>Personal Information</h3>
            <div className={styles.infoGrid}>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Email:</span>
                <span className={styles.infoValue}>{user.Email}</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Name:</span>
                <span className={styles.infoValue}>{user.FullName || 'N/A'}</span>
              </div>
            </div>
          </section>
        )}
      </main>

      {/* Booking Modal */}
      {showBookingModal && (
        <div
          className={styles.modalOverlay}
        >
          <div
            className={styles.modalContent}
            onClick={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 className={styles.modalTitle}>
                Book {selectedItem?.Title || 'Item'}
              </h3>
              <button
                className={styles.modalCloseBtn}
                onClick={handleCloseModal}
              >
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
                  onChange={(e) => setStartDate(e.target.value)}
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
                  onChange={(e) => setEndDate(e.target.value)}
                  className={styles.modalInput}
                  min={startDate || new Date().toISOString().slice(0, 10)}
                  required
                />
              </div>

              {selectedItem?.DailyRate && (
                <div className={styles.priceInfo}>
                  <span className={styles.priceLabel}>Estimated Total:</span>
                  <span className={styles.priceValue}>
                    $
                    {(
                      selectedItem.DailyRate *
                      Math.max(
                        1,
                        Math.ceil(
                          (new Date(endDate) - new Date(startDate)) /
                            (1000 * 60 * 60 * 24),
                        ),
                      )
                    ).toFixed(2)}
                  </span>
                </div>
              )}
            </div>

            <div className={styles.modalFooter}>
              <button
                className={styles.modalCancelBtn}
                onClick={handleCloseModal}
                disabled={isBooking}
              >
                Cancel
              </button>
              <button
                className={styles.modalSubmitBtn}
                onClick={handleBookingSubmit}
                disabled={isBooking || !startDate || !endDate}
              >
                {isBooking ? 'Processing...' : 'Confirm Booking'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Review Modal */}
      {showReviewModal && (
        <div className={styles.modalOverlay}>
          <div
            className={styles.modalContent}
            onClick={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 className={styles.modalTitle}>
                Write a Review for {reviewBooking?.item?.Title || 'Item'}
              </h3>
              <button
                className={styles.modalCloseBtn}
                onClick={handleCloseReviewModal}
              >
                ×
              </button>
            </div>
            <div className={styles.modalBody}>
              {reviewError && (
                <div className={styles.errorMessage} role="alert">
                  {reviewError}
                </div>
              )}
              {reviewMessage && (
                <output className={styles.successMessage}>
                  {reviewMessage}
                </output>
              )}
              <div className={styles.formGroup}>
                <label className={styles.modalLabel}>Rating</label>
                <input
                  type="number"
                  min="1"
                  max="5"
                  value={reviewRating}
                  onChange={(e) => setReviewRating(parseInt(e.target.value, 10))}
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
                  value={reviewComment}
                  onChange={(e) => setReviewComment(e.target.value)}
                  className={styles.modalTextarea}
                />
              </div>
            </div>
            <div className={styles.modalFooter}>
              <button
                className={styles.modalCancelBtn}
                onClick={handleCloseReviewModal}
              >
                Cancel
              </button>
              <button
                className={styles.modalSubmitBtn}
                onClick={handleReviewSubmit}
                disabled={reviewRating === 0}
              >
                Submit Review
              </button>
            </div>
          </div>
        </div>
      )}

      {showDeleteModal && (
        <div className={styles.modalOverlay}>
          <div
            className={styles.modalContent}
            onClick={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 className={styles.modalTitle}>Delete item</h3>
              <button
                className={styles.modalCloseBtn}
                onClick={cancelDelete}
              >
                ×
              </button>
            </div>
            <div className={styles.modalBody}>
              <p>
                Are you sure you want to delete "
                {itemToDelete?.Title || 'this item'}"?
              </p>
              {deleteError && (
                <div className={styles.errorMessage} role="alert">
                  {deleteError}
                </div>
              )}
            </div>
            <div className={styles.modalFooter}>
              <button
                className={styles.modalCancelBtn}
                onClick={cancelDelete}
              >
                Cancel
              </button>
              <button
                className={styles.modalSubmitBtn}
                onClick={confirmDeleteItem}
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
