import { useState, useEffect } from 'react'
import axios from 'axios'
import styles from './Dashboard.module.css'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

export default function DashboardPage() {
  const [user, setUser] = useState(null)
  const [items, setItems] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    // Get user from localStorage
    const storedUser = localStorage.getItem('currentUser')
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser))
      } catch (err) {
        console.error('Failed to parse user:', err)
      }
    }

    // Check if we need to refresh items (e.g., after adding a new item)
    const shouldRefresh = localStorage.getItem('refreshItems') === 'true'
    if (shouldRefresh) {
      localStorage.removeItem('refreshItems')
    }

    // Fetch items
    fetchItems()
  }, [])

  const fetchItems = async () => {
    try {
      setIsLoading(true)
      const response = await axios.get(`${API_BASE_URL}/items`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
        },
      })

      const data = response.data
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        setError(err.response.data?.message ?? 'Failed to fetch items')
      } else {
        setError(err.message || 'An unexpected error occurred.')
      }
      console.error('Error fetching items:', err)
    } finally {
      setIsLoading(false)
    }
  }

  const handleLogout = () => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('currentUser')
    window.location.href = '/'
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
        <section className={styles.welcome}>
          <h2 className={styles.welcomeTitle}>
            Welcome, {user?.name || user?.email || 'Utilizator'}!
          </h2>
          <p className={styles.welcomeSubtitle}>
            Hello! You are connected on UniShare.
          </p>
        </section>

        <section className={styles.itemsSection}>
          <div className={styles.sectionHeader}>
            <h3 className={styles.sectionTitle}>Items available</h3>
            <button 
              className={styles.addItemBtn}
              onClick={() => window.location.href = '/add-item'}
            >
              + Add Item
            </button>
          </div>

          {error && (
            <div className={styles.errorMessage} role="alert">
              {error}
            </div>
          )}

          {isLoading ? (
            <div className={styles.loadingMessage}>Loading Items...</div>
          ) : items.length === 0 ? (
            <div className={styles.emptyMessage}>
              There are no items available at this time.
            </div>
          ) : (
            <div className={styles.itemsGrid}>
              {items.map((item) => (
                <div key={item.id} className={styles.itemCard}>
                  <div className={styles.itemHeader}>
                    <h4 className={styles.itemTitle}>{item.title || 'Fără titlu'}</h4>
                    <span className={styles.itemBadge}>{item.category || 'General'}</span>
                  </div>
                  <p className={styles.itemDescription}>
                    {item.description || 'Fără descriere'}
                  </p>
                  <div className={styles.itemFooter}>
                    <span className={styles.itemOwner}>
                      {item.ownerName || 'Utilizator necunoscut'}
                    </span>
                    <button className={styles.borrowBtn}>Borrow</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>

        {user && (
          <section className={styles.userInfo}>
            <h3 className={styles.infoTitle}>Personal Information</h3>
            <div className={styles.infoGrid}>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Email:</span>
                <span className={styles.infoValue}>{user.email}</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Name:</span>
                <span className={styles.infoValue}>{user.name || 'N/A'}</span>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  )
}
