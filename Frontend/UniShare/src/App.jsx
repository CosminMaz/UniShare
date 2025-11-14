import './App.css'
import LoginPage from './pages/Login'
import RegisterPage from './pages/Register'
import DashboardPage from './pages/Dashboard'

function App() {
  const path = window.location.pathname

  if (path === '/register') {
    return <RegisterPage />
  }

  if (path === '/dashboard') {
    return <DashboardPage />
  }

  return <LoginPage />
}

export default App
