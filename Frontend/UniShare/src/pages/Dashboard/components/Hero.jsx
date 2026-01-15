import styles from '../Dashboard.module.css'

export function Hero({ user, stats, onAddItem }) {
  return (
    <section className={styles.hero}>
      <div className={styles.heroContent}>
        <p className={styles.heroEyebrow}>Dashboard</p>
        <h2 className={styles.heroTitle}>
          Welcome, {user?.FullName || user?.Email || 'UniShare member'} 👋
        </h2>
        <p className={styles.heroSubtitle}>
          Keep lending momentum going—review requests, connect with borrowers,
          and showcase the items that make life easier for fellow students.
        </p>
        <div className={styles.heroActions}>
          <button className={styles.primaryBtn} onClick={onAddItem}>
            List a New Item
          </button>
        </div>
      </div>
      <div className={styles.heroStats}>
        {stats.map((stat) => (
          <div key={stat.label} className={styles.statCard}>
            <span className={styles.statLabel}>{stat.label}</span>
            <span className={styles.statValue}>{stat.value}</span>
            <span className={styles.statHint}>{stat.hint}</span>
          </div>
        ))}
      </div>
    </section>
  )
}
