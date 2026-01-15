import { useState } from 'react'
import axios from 'axios'
import styles from './AddItemPage.module.css'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

// Map category names to enum values
const CATEGORY_MAP = {
  'Books': 'Books',
  'Electronics': 'Electronics',
  'Clothing': 'Clothing',
  'Furniture': 'Furniture',
  'Sports': 'Sports',
  'Other': 'Other'
}

const CATEGORIES = Object.keys(CATEGORY_MAP)

// Map condition names to enum values
const CONDITION_MAP = {
  'New': 'New',
  'Like New': 'LikeNew',
  'Well Preserved': 'WellPreserved',
  'Acceptable': 'Acceptable',
  'Needs Repairs': 'Poor'
}

const CONDITIONS = Object.keys(CONDITION_MAP)

export default function AddItemPage() {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: 'Books',
    condition: 'Well Preserved',
    dailyRate: '',
    imageUrl: ''
  })

  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)
  const creationSteps = [
    'Describe what you are lending with a memorable title.',
    'Share the exact condition and daily rate to set expectations.',
    'Provide a hosted image link so your listing stands out.',
  ]
  const quickTips = [
    'Use daylight photos for better visibility.',
    'Add pickup instructions inside the description field.',
    'Pause or edit listings whenever availability changes.',
  ]

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    setError('')
  }

  const validateForm = () => {
    if (!formData.title.trim()) {
      setError('Item name is required')
      return false
    }
    if (formData.title.trim().length < 3) {
      setError('Item name must be at least 3 characters long')
      return false
    }
    if (!formData.description.trim()) {
      setError('Description is required')
      return false
    }
    if (formData.description.trim().length < 5) {
      setError('Description must be at least 5 characters long')
      return false
    }
    if (!formData.category) {
      setError('Please select a category')
      return false
    }
    if (!formData.condition) {
      setError('Please select a condition')
      return false
    }
    if (!formData.dailyRate) {
      setError('Daily rate is required')
      return false
    }
    const rate = Number.parseFloat(formData.dailyRate)
    if (Number.isNaN(rate) || rate <= 0) {
      setError('Daily rate must be a positive number')
      return false
    }
    if (!formData.imageUrl.trim()) {
      setError('Image URL is required')
      return false
    }
    try {
      new URL(formData.imageUrl.trim())
    } catch {
      setError('Image URL is not valid')
      return false
    }
    return true
  }

  const handleSubmit = async (e) => {
  e.preventDefault()

  if (!validateForm()) {
    return
  }

  setIsLoading(true)
  setError('')
  setSuccess(false)

  try {
    // 1. luam user-ul din localStorage
    const storedUser = localStorage.getItem('currentUser')
    if (!storedUser) {
      setError('User not found. Please log in again.')
      setIsLoading(false)
      return
    }
    const user = JSON.parse(storedUser)

    // 2. luam token-ul (suporta atat "accessToken" cat si "token")
    const token =
      localStorage.getItem('accessToken') ?? localStorage.getItem('token')

    if (!token) {
      setError('You are not authenticated. Please log in.')
      setIsLoading(false)
      return
    }

    const payload = {
      ownerId: user.id ?? user.Id, // depinde cum vine din backend
      title: formData.title.trim(),
      description: formData.description.trim(),
      categ: CATEGORY_MAP[formData.category],
      cond: CONDITION_MAP[formData.condition],
      dailyRate: Number.parseFloat(formData.dailyRate),
      imageUrl: formData.imageUrl.trim()
    }

    await axios.post(`${API_BASE_URL}/items`, payload, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    })

    setSuccess(true)
    setFormData({
      title: '',
      description: '',
      category: 'Books',
      condition: 'Well Preserved',
      dailyRate: '',
      imageUrl: ''
    })

    // Set flag to refresh items in dashboard
    localStorage.setItem('refreshItems', 'true')

    setTimeout(() => {
      globalThis.location.href = '/dashboard'
    }, 1500)
  } catch (err) {
    console.error('Error adding item:', err)
    console.error('Full error response:', err.response?.data)

    if (axios.isAxiosError(err) && err.response) {
      const errorData = err.response.data
      let errorMessage = 'An error occurred while adding the item'

      if (errorData?.message) {
        errorMessage = errorData.message
      } else if (errorData?.errors) {
        errorMessage = Object.entries(errorData.errors)
          .map(([key, values]) => `${key}: ${values.join(', ')}`)
          .join('; ')
      } else if (err.response.status === 401) {
        errorMessage = 'You are not authenticated. Please log in again.'
      } else if (err.response.status === 400) {
        errorMessage = errorData?.title || 'Invalid request data'
      }

      setError(errorMessage)
    } else {
      setError(err.message || 'An unexpected error occurred')
    }
  } finally {
    setIsLoading(false)
  }
}


  const handleCancel = () => {
    globalThis.location.href = '/dashboard'
  }

  return (
    <div className={styles.container}>
      <nav className={styles.navbar}>
        <div className={styles.navContent}>
          <a href="/dashboard" style={{ color: 'inherit', textDecoration: 'none' }}>
            <h1 className={styles.title}>UniShare</h1>
          </a>
          <button className={styles.backBtn} onClick={handleCancel}>
            ← Back to Dashboard
          </button>
        </div>
      </nav>

      <main className={styles.main}>
        <section className={styles.infoPanel}>
          <span className={styles.infoBadge}>Share smarter</span>
          <h2 className={styles.infoTitle}>Add new gear to your lending vault</h2>
          <p className={styles.infoSubtitle}>
            Every listing powers a new project. Keep details crisp so borrowers know exactly what to expect.
          </p>
          <div>
            <h3>How it works</h3>
            <ul className={styles.stepsList}>
              {creationSteps.map((step, index) => (
                <li key={step}>
                  <div className={styles.stepIndex}>{index + 1}</div>
                  <p>{step}</p>
                </li>
              ))}
            </ul>
          </div>
          <div>
            <h3>Pro tips</h3>
            <ul className={styles.tipsList}>
              {quickTips.map((tip) => (
                <li key={tip}>
                  <div className={styles.stepIndex}>★</div>
                  <p>{tip}</p>
                </li>
              ))}
            </ul>
          </div>
        </section>

        <section className={styles.formCard}>
          <div className={styles.formHeader}>
            <h2 className={styles.formTitle}>Add a New Item</h2>
            <p className={styles.formSubtitle}>
              Fill out the form below to add a new item to your list
            </p>
          </div>

          {error && (
            <div className={styles.errorMessage} role="alert">
              {error}
            </div>
          )}

          {success && (
            <div className={styles.successMessage} role="status">
              ✓ Item added successfully! Redirecting...
            </div>
          )}

          <form onSubmit={handleSubmit} className={styles.form}>
            <div className={styles.formGroup}>
              <label htmlFor="title" className={styles.label}>
                Item Name
                <span className={styles.required}>*</span>
              </label>
              <input
                type="text"
                id="title"
                name="title"
                value={formData.title}
                onChange={handleChange}
                placeholder="ex: Romanian Language Book - Grade 10"
                className={styles.input}
                disabled={isLoading}
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="description" className={styles.label}>
                Description
                <span className={styles.required}>*</span>
              </label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="ex: Book in very good condition, lightly used, contains all solved exercises..."
                className={styles.textarea}
                disabled={isLoading}
              />
            </div>

            <div className={styles.formRow}>
              <div className={styles.formGroup}>
                <label htmlFor="category" className={styles.label}>
                  Category
                  <span className={styles.required}>*</span>
                </label>
                <select
                  id="category"
                  name="category"
                  value={formData.category}
                  onChange={handleChange}
                  className={styles.select}
                  disabled={isLoading}
                >
                  {CATEGORIES.map(cat => (
                    <option key={cat} value={cat}>
                      {cat}
                    </option>
                  ))}
                </select>
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="condition" className={styles.label}>
                  Condition
                  <span className={styles.required}>*</span>
                </label>
                <select
                  id="condition"
                  name="condition"
                  value={formData.condition}
                  onChange={handleChange}
                  className={styles.select}
                  disabled={isLoading}
                >
                  {CONDITIONS.map(cond => (
                    <option key={cond} value={cond}>
                      {cond}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="dailyRate" className={styles.label}>
                Daily Rate (RON/day)
                <span className={styles.required}>*</span>
              </label>
              <input
                type="number"
                id="dailyRate"
                name="dailyRate"
                value={formData.dailyRate}
                onChange={handleChange}
                placeholder="ex: 5.50"
                step="0.01"
                min="0"
                className={styles.input}
                disabled={isLoading}
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="imageUrl" className={styles.label}>
                Image URL
                <span className={styles.required}>*</span>
              </label>
              <input
                type="url"
                id="imageUrl"
                name="imageUrl"
                value={formData.imageUrl}
                onChange={handleChange}
                placeholder="ex: https://example.com/image.jpg"
                className={styles.input}
                disabled={isLoading}
              />
            </div>

            <div className={styles.buttonGroup}>
              <button
                type="button"
                className={styles.cancelBtn}
                onClick={handleCancel}
                disabled={isLoading}
              >
                Cancel
              </button>
              <button
                type="submit"
                className={styles.submitBtn}
                disabled={isLoading}
              >
                {isLoading ? (
                  <>
                    <span className={styles.loadingSpinner} />
                    Adding...
                  </>
                ) : (
                  '✓ Add Item'
                )}
              </button>
            </div>
          </form>
        </section>
      </main>
    </div>
  )
}
