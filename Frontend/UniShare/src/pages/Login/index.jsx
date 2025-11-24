import { useState } from 'react'
import styles from './Login.module.css'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      })

      if (!response.ok) {
        const body = await response.json().catch(() => ({}))
        throw new Error(body?.message ?? 'Email sau parolă incorectă')
      }

      const data = await response.json()
      const { token, user } = data

      if (token) {
        localStorage.setItem('accessToken', token)
      }

      if (user) {
        localStorage.setItem('currentUser', JSON.stringify(user))
      }

      // TODO: Redirect to dashboard route when available
      window.location.href = '/dashboard'
    } catch (err) {
      setError(err.message)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <header className={styles.header}>
          <h1 className={styles.headerTitle}>UniShare</h1>
          <p className={styles.headerSubtitle}>Student lending platform</p>
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
            {isLoading ? 'Se autentifică...' : 'Autentificare'}
          </button>

          <footer className={styles.footer}>
            <p>
              No account?{' '}
              <a className={styles.footerLink} href="/register">
                Register
              </a>
            </p>
          </footer>
        </form>
      </div>
    </div>
  )
}

