import { useState } from 'react'
import styles from './Register.module.css'

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
      const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          fullname,
          email,
          password,
        }),
      })

      if (!response.ok) {
        const body = await response.json().catch(() => ({}))
        
        // Treat the errors from the backend
        if (response.status === 400 && body.errors) {
          const errorMessages = Object.values(body.errors).flat()
          throw new Error(errorMessages.join(', ') || 'Date invalide')
        }
        
        throw new Error(body?.title || body?.message || 'Eroare la înregistrare')
      }

      setSuccess(true)
      
      // Redirect to login page after 2 seconds
      setTimeout(() => {
        window.location.href = '/login'
      }, 2000)
    } catch (err) {
      setError(err.message)
    } finally {
      setIsLoading(false)
    }
  }

  if (success) {
    return (
      <div className={styles.container}>
        <div className={styles.card}>
          <div className={styles.successMessage}>
            <h2>Cont creat cu succes!</h2>
            <p>Ești redirecționat către pagina de login...</p>
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
          <p className={styles.headerSubtitle}>Creează-ți contul</p>
        </header>

        <form className={styles.form} onSubmit={handleSubmit}>
          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="fullname">
              Nume complet
            </label>
            <input
              id="fullname"
              type="text"
              className={styles.input}
              placeholder="Prenume Nume"
              value={fullname}
              onChange={(event) => setFullname(event.target.value)}
              required
              minLength={3}
              disabled={isLoading}
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="email">
              Email instituțional
            </label>
            <input
              id="email"
              type="email"
              className={styles.input}
              placeholder="prenume.nume@university.ro"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="password">
              Parolă
            </label>
            <input
              id="password"
              type="password"
              className={styles.input}
              placeholder="Minim 6 caractere"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              minLength={6}
              disabled={isLoading}
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.label} htmlFor="confirmPassword">
              Confirmă parola
            </label>
            <input
              id="confirmPassword"
              type="password"
              className={styles.input}
              placeholder="Reintrodu parola"
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
            {isLoading ? 'Se înregistrează...' : 'Înregistrare'}
          </button>

          <footer className={styles.footer}>
            <p>
              Ai deja cont?{' '}
              <a className={styles.footerLink} href="/login">
                Autentifică-te
              </a>
            </p>
          </footer>
        </form>
      </div>
    </div>
  )
}

