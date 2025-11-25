import { useState } from 'react'
import styles from './Register.module.css'
import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5222'

export default function RegisterPage() {
  const [fullname, setFullname] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')
    setSuccess(false)

    // Frontend validation
    if (password !== confirmPassword) {
      setError('Parolele nu coincid')
      return
    }

    if (fullname.length < 3) {
      setError('Numele trebuie să aibă cel puțin 3 caractere')
      return
    }

    if (password.length < 6) {
      setError('Parola trebuie să aibă cel puțin 6 caractere')
      return
    }

    setIsLoading(true)

    try {
      await axios.post(`${API_BASE_URL}/api/auth/register`, {
        fullname,
        email,
        password,
      })

      setSuccess(true)

      // Redirect to login page after 2 seconds
      setTimeout(() => {
        window.location.href = '/login'
      }, 2000)
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        const { data, status } = err.response
        if (status === 400 && data.errors) {
          setError(Object.values(data.errors).flat().join(', ') || 'Date invalide')
        } else {
          setError(data?.title || data?.message || 'Eroare la înregistrare')
        }
      } else {
        setError(err.message || 'A apărut o eroare neașteptată.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  if (success) {
    return (
      <div className={styles.container}>
        <div className={styles.card}>
          <div className={styles.successMessage}>
            <h2>Account created successfully!</h2>
            <p>You are redirected to the login page...</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <header className={styles.header}>
          <h1 className={styles.headerTitle}>UniShare</h1>
          <p className={styles.headerSubtitle}>Create account</p>
        </header>

        <form className={styles.form} onSubmit={handleSubmit}>
          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="fullname">
              Complete Name
            </label>
            <input
              id="fullname"
              type="text"
              className={styles.input}
              placeholder="FirstName LastName"
              value={fullname}
              onChange={(event) => setFullname(event.target.value)}
              required
              minLength={3}
              disabled={isLoading}
            />
          </div>

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
              placeholder="At least 6 characters"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              minLength={6}
              disabled={isLoading}
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="confirmPassword">
              Confirm Password
            </label>
            <input
              id="confirmPassword"
              type="password"
              className={styles.input}
              placeholder="Re-enter the password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
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
            {isLoading ? 'Registering...' : 'Register'}
          </button>

          <footer className={styles.footer}>
            <p>
              Already have an account?{' '}
              <a className={styles.footerLink} href="/login">
                Login
              </a>
            </p>
          </footer>
        </form>
      </div>
    </div>
  )
}
