import { useState, useEffect } from 'react'
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

    // Fetch items
    fetchItems()
  }, [])

  const fetchItems = async () => {
    try {
      setIsLoading(true)
      const response = await fetch(`${API_BASE_URL}/items`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
        },
      })

      if (!response.ok) {
        throw new Error('Failed to fetch items')
      }

      const data = await response.json()
      setItems(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err.message)
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
            Deconectare
          </button>
        </div>
      </nav>

      <main className={styles.main}>
        <section className={styles.welcome}>
          <h2 className={styles.welcomeTitle}>
            Bine ai venit, {user?.name || user?.email || 'Utilizator'}!
          </h2>
          <p className={styles.welcomeSubtitle}>
            Bună! Esti conectat la platforma UniShare
          </p>
        </section>

        <section className={styles.itemsSection}>
          <div className={styles.sectionHeader}>
            <h3 className={styles.sectionTitle}>Articole disponibile</h3>
            <button className={styles.addItemBtn}>+ Adaugă articol</button>
          </div>

          {error && (
            <div className={styles.errorMessage} role="alert">
              {error}
            </div>
          )}

          {isLoading ? (
            <div className={styles.loadingMessage}>Se încarcă articolele...</div>
          ) : items.length === 0 ? (
            <div className={styles.emptyMessage}>
              Nu sunt articole disponibile în acest moment.
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
                    <button className={styles.borrowBtn}>Imprumută</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>

        {user && (
          <section className={styles.userInfo}>
            <h3 className={styles.infoTitle}>Informații personale</h3>
            <div className={styles.infoGrid}>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Email:</span>
                <span className={styles.infoValue}>{user.email}</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Nume:</span>
                <span className={styles.infoValue}>{user.name || 'N/A'}</span>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  )
}
