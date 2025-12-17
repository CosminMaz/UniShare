import { useState } from 'react'
import styles from './Login.module.css'
import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const highlights = [
    {
      title: 'Campus-first lending',
      description: 'Borrow lab gear, books, or gadgets from nearby students.',
    },
    {
      title: 'Trust & transparency',
      description: 'Ratings and reviews after each booking keep the community safe.',
    },
    {
      title: 'Track it all',
      description: 'Approve requests, monitor loans, and stay in control of your listings.',
    },
  ]

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      const response = await axios.post(`${API_BASE_URL}/api/auth/login`, {
        email,
        password,
      })

      
      const rawData = response.data
      const token = rawData.token ?? rawData.Token
      const user = rawData.user ?? rawData.User

      if (token) {
        localStorage.setItem('token', token)
      }

      if (user) {
        localStorage.setItem('currentUser', JSON.stringify(user))
      }

      window.location.href = '/dashboard'
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        // Use the error message from the API, or a default one
        setError(err.response.data?.message ?? 'Email sau parolă incorectă')
      } else {
        setError(err.message || 'A apărut o eroare neașteptată.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className={styles.container}>
      <div className={styles.gridShell}>
        <section className={styles.brandPanel}>
          <div className={styles.logoBadge}>UniShare</div>
          <h2 className={styles.brandTitle}>
            Join the most stylish way to lend across campus.
          </h2>
          <p className={styles.brandSubtitle}>
            Borrow what you need, lend what you love, and keep the student community moving.
          </p>
          <ul className={styles.highlightList}>
            {highlights.map((item) => (
              <li key={item.title}>
                <div className={styles.highlightIcon}>✦</div>
                <div>
                  <p className={styles.highlightTitle}>{item.title}</p>
                  <p className={styles.highlightDescription}>{item.description}</p>
                </div>
              </li>
            ))}
          </ul>
        </section>

        <section className={styles.card}>
          <header className={styles.header}>
            <p className={styles.headerEyebrow}>Sign in</p>
            <h1 className={styles.headerTitle}>Welcome back</h1>
            <p className={styles.headerSubtitle}>
              Continue sharing everyday essentials with fellow students.
            </p>
          </header>

          <form className={styles.form} onSubmit={handleSubmit}>
            <div className={styles.formGroup}>
              <label className={styles.label} htmlFor="email">
                Institutional Email
              </label>
              <input
                id="email"
                type="email"
                className={styles.input}
                placeholder="example@university.ro"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
                disabled={isLoading}
              />
            </div>

            <div className={styles.formGroup}>
              <label className={styles.label} htmlFor="password">
                Password
              </label>
              <input
                id="password"
                type="password"
                className={styles.input}
                placeholder="Your password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
                minLength={6}
                disabled={isLoading}
              />
            </div>

            {error && (
              <p className={styles.errorMessage} role="alert">
                {error}
              </p>
            )}

            <button type="submit" className={styles.button} disabled={isLoading}>
              {isLoading ? 'Signing you in...' : 'Sign in'}
            </button>

            <footer className={styles.footer}>
              <p>
                No account?{' '}
                <a className={styles.footerLink} href="/register">
                  Create one now
                </a>
              </p>
            </footer>
          </form>
        </section>
      </div>
    </div>
  )
}
