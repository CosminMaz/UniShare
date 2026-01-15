import './App.css'
import LoginPage from './pages/Login'
import RegisterPage from './pages/Register'
import DashboardPage from './pages/Dashboard'
import AddItemPage from './pages/AddItemPage'

function App() {
  const path = globalThis.location.pathname

  if (path === '/register') {
    return <RegisterPage />
  }

  if (path === '/dashboard') {
    return <DashboardPage />
  }

  if (path === '/add-item') {
    return <AddItemPage />
  }

  return <LoginPage />
}

export default App
