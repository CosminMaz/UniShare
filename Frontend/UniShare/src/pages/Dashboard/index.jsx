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
            Welcome, {user?.FullName || user?.Email || 'Utilizator'}!
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
              onClick={() => (window.location.href = '/add-item')}
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
              {items.map(item => (
                <div key={item.Id} className={styles.itemCard}>
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
                      {item.Title || 'Fara titlu'}
                    </h4>
                    <span className={styles.itemBadge}>
                      {item.Categ || 'General'}
                    </span>
                  </div>
                  <p className={styles.itemDescription}>
                    {item.Description || 'Fara descriere'}
                  </p>
                  <div className={styles.itemDetails}>
                    <div className={styles.detailRow}>
                      <span className={styles.detailLabel}>Stare:</span>
                      <span className={styles.detailValue}>
                        {item.Cond || 'N/A'}
                      </span>
                    </div>
                    <div className={styles.detailRow}>
                      <span className={styles.detailLabel}>Pret/zi:</span>
                      <span className={styles.detailValue}>
                        {item.DailyRate
                          ? `$${item.DailyRate.toFixed(2)}`
                          : 'N/A'}
                      </span>
                    </div>
                    <div className={styles.detailRow}>
                      <span className={styles.detailLabel}>Adaugat:</span>
                      <span className={styles.detailValue}>
                        {item.CreatedAt
                          ? new Date(item.CreatedAt).toLocaleDateString('ro-RO')
                          : 'N/A'}
                      </span>
                    </div>
                  </div>
                  <div className={styles.itemFooter}>
                    <button className={styles.borrowBtn}>Imprumuta</button>
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
    </div>
  )
}
