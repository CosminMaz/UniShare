import { useCallback, useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'
import styles from './Dashboard.module.css'
import { BookingModal } from './components/BookingModal'
import { BookingsSection } from './components/BookingsSection'
import { DeleteModal } from './components/DeleteModal'
import { Hero } from './components/Hero'
import { ItemsSection } from './components/ItemsSection'
import { MyItemsSection } from './components/MyItemsSection'
import { OwnerRequestsSection } from './components/OwnerRequestsSection'
import { ReviewModal } from './components/ReviewModal'
import { ReviewsSection } from './components/ReviewsSection'
import {
  getApiErrorMessage,
  getBookingStatus,
  getReviewTypeFromRating,
  normalizeId,
} from './utils'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

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

const formatStatValue = (value) => Number(value ?? 0).toLocaleString('en-US')

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
  const [showReviewModal, setShowReviewModal] = useState(false)
  const [reviewBooking, setReviewBooking] = useState(null)
  const [reviewRating, setReviewRating] = useState(0)
  const [reviewComment, setReviewComment] = useState('')
  const [reviewError, setReviewError] = useState('')
  const [reviewMessage, setReviewMessage] = useState('')

  const currentUserId = user?.Id ?? user?.id

  const fetchItems = async () => {
    try {
      setIsLoading(true)
      const response = await axios.get(`${API_BASE_URL}/items`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
        },
      })

      const data = response.data
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to fetch items'))
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

      if (!token) {
        setBookings([])
        setOwnerBookings([])
        return
      }

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
      } catch (parseError) {
        console.error('Failed to parse user for booking filter:', parseError)
        setBookings(allBookings)
        setOwnerBookings([])
      }
    } catch (err) {
      // Don't surface booking fetch errors to the UI; log instead
      console.error('Error fetching bookings:', err)
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
    const storedUser = localStorage.getItem('currentUser')
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser))
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

    const ownerId = itemToDelete.OwnerId ?? itemToDelete.ownerId
    if (!currentUserId || currentUserId !== ownerId) {
      setDeleteError('You can only delete items you posted.')
      return
    }

    try {
      setDeleteError('')
      setDeleteMessage('')

      await axios.delete(
        `${API_BASE_URL}/items/${itemToDelete.Id ?? itemToDelete.id}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      )

      setDeleteMessage('Item deleted successfully.')
      setShowDeleteModal(false)
      setItemToDelete(null)
      fetchItems()
      fetchMyItems()
    } catch (err) {
      setDeleteError(
        getApiErrorMessage(err, 'Failed to delete the item. Please try again.'),
      )
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
    const defaultEnd = new Date(today.getTime() + 2 * 24 * 60 * 60 * 1000)
      .toISOString()
      .slice(0, 10)

    setSelectedItem(item)
    setStartDate(defaultStart)
    setEndDate(defaultEnd)
    setShowBookingModal(true)
    setError('')
    setBookingMessage('')
  }

  const resetBookingState = () => {
    setShowBookingModal(false)
    setSelectedItem(null)
    setStartDate('')
    setEndDate('')
    setError('')
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
      resetBookingState()
      fetchBookings()
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to create booking.'))
    } finally {
      setIsBooking(false)
    }
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
      fetchItems()
      fetchMyItems()

      setTimeout(() => setBookingRequestMessage(''), 3000)
    } catch (err) {
      setBookingRequestError(
        getApiErrorMessage(err, 'Failed to approve booking.'),
      )
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
      setTimeout(() => setBookingRequestMessage(''), 3000)
    } catch (err) {
      setBookingRequestError(
        getApiErrorMessage(err, 'Failed to reject booking.'),
      )
    }
  }

  const handleCompleteBooking = async (bookingId) => {
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
        `${API_BASE_URL}/bookings/${bookingId}/complete`,
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      )

      setBookingRequestMessage('Booking marked as returned!')
      fetchBookings()
      fetchItems()
      fetchMyItems()

      setTimeout(() => setBookingRequestMessage(''), 3000)
    } catch (err) {
      setBookingRequestError(
        getApiErrorMessage(err, 'Failed to mark booking as returned.'),
      )
    }
  }

  const handleWriteReviewClick = (booking) => {
    setReviewBooking(booking)
    setShowReviewModal(true)
    setReviewError('')
    setReviewMessage('')
    setReviewRating(0)
    setReviewComment('')
  }

  const handleCloseReviewModal = () => {
    setShowReviewModal(false)
    setReviewBooking(null)
  }

  const handleReviewSubmit = async () => {
    if (!reviewBooking || reviewRating === 0) {
      setReviewError('Please provide a rating.')
      return
    }

    const token = localStorage.getItem('accessToken') ?? localStorage.getItem('token')
    if (!token) {
      setReviewError('You need to be logged in to leave a review.')
      return
    }

    const reviewerId = currentUserId
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
      }

      await axios.post(`${API_BASE_URL}/reviews`, payload, {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      })

      setReviewMessage('Thank you for your review!')
      fetchReviews()
      setTimeout(() => {
        handleCloseReviewModal()
      }, 2000)
    } catch (err) {
      setReviewError(getApiErrorMessage(err, 'Failed to submit review.'))
    }
  }

  const handleCloseModal = () => {
    resetBookingState()
  }

  const handleNavigateToAddItem = () => {
    globalThis.location.href = '/add-item'
  }

  const handleLogout = () => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('currentUser')
    globalThis.location.href = '/'
  }

  const hasSubmittedReview = useCallback(
    (bookingId) => {
      const targetId = normalizeId(bookingId)
      return reviews.some(
        (review) => normalizeId(review.BookingId ?? review.bookingId) === targetId,
      )
    },
    [reviews],
  )

  const activeBorrowings = bookings.filter((booking) => {
    const status = getBookingStatus(booking)
    return status === 'Approved' || status === 'Active'
  }).length

  const pendingOwnerRequests = ownerBookings.filter(
    (booking) => getBookingStatus(booking) === 'Pending',
  ).length

  const dashboardStats = useMemo(
    () => [
      { label: 'Marketplace Items', value: formatStatValue(items.length), hint: 'Live listings' },
      { label: 'My Listings', value: formatStatValue(myItems.length), hint: 'Shared with students' },
      { label: 'Active Borrowings', value: formatStatValue(activeBorrowings), hint: 'In progress' },
      { label: 'Pending Requests', value: formatStatValue(pendingOwnerRequests), hint: 'Need review' },
    ],
    [items.length, myItems.length, activeBorrowings, pendingOwnerRequests],
  )

  const reviewItemTitle = useMemo(() => {
    if (!reviewBooking) return ''
    const match = items.find(
      (item) => normalizeId(item.Id ?? item.id) === normalizeId(reviewBooking.ItemId ?? reviewBooking.itemId),
    )
    return match?.Title || match?.title || 'Item'
  }, [items, reviewBooking])

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
        <Hero user={user} stats={dashboardStats} onAddItem={handleNavigateToAddItem} />

        <div className={styles.marketplaceColumn}>
          <ItemsSection
            title="Items available"
            subtitle="Discover what the UniShare community is lending today"
            items={items}
            isLoading={isLoading}
            errors={[error, deleteError]}
            successes={[bookingMessage, deleteMessage]}
            onBorrow={handleBorrowClick}
            onDelete={handleDeleteItem}
            isBooking={isBooking}
            currentUserId={currentUserId}
          />

          <MyItemsSection
            items={myItems}
            isLoading={isLoadingMyItems}
            error={myItemsError}
            onDelete={handleDeleteItem}
          />
        </div>

        <div className={styles.bookingsGrid}>
          <BookingsSection
            bookings={bookings}
            items={items}
            isLoading={isLoadingBookings}
            hasSubmittedReview={hasSubmittedReview}
            onWriteReview={handleWriteReviewClick}
          />

          <OwnerRequestsSection
            bookings={ownerBookings}
            items={items}
            isLoading={isLoadingBookings}
            onApprove={handleApproveBooking}
            onReject={handleRejectBooking}
            onComplete={handleCompleteBooking}
            error={bookingRequestError}
            message={bookingRequestMessage}
          />
        </div>

        <ReviewsSection reviews={reviews} items={items} />

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

      <BookingModal
        isOpen={showBookingModal}
        selectedItem={selectedItem}
        startDate={startDate}
        endDate={endDate}
        onStartDateChange={setStartDate}
        onEndDateChange={setEndDate}
        onClose={handleCloseModal}
        onSubmit={handleBookingSubmit}
        isBooking={isBooking}
        error={error}
      />

      <ReviewModal
        isOpen={showReviewModal}
        itemTitle={reviewItemTitle || reviewBooking?.item?.Title}
        rating={reviewRating}
        comment={reviewComment}
        error={reviewError}
        message={reviewMessage}
        onRatingChange={setReviewRating}
        onCommentChange={setReviewComment}
        onClose={handleCloseReviewModal}
        onSubmit={handleReviewSubmit}
      />

      <DeleteModal
        isOpen={showDeleteModal}
        itemTitle={itemToDelete?.Title}
        error={deleteError}
        onCancel={cancelDelete}
        onConfirm={confirmDeleteItem}
      />
    </div>
  )
}
