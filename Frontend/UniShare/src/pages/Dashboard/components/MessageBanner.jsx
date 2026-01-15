import styles from '../Dashboard.module.css'

export function MessageBanner({ type = 'info', children }) {
  if (!children) return null

  const isError = type === 'error'
  const Component = isError ? 'div' : 'output'

  return (
    <Component
      className={isError ? styles.errorMessage : styles.successMessage}
      role={isError ? 'alert' : undefined}
    >
      {children}
    </Component>
  )
}
