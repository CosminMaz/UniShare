import './App.css'
import LoginPage from './pages/Login'
import RegisterPage from './pages/Register'

function App() {
  const path = window.location.pathname

  if (path === '/register') {
    return <RegisterPage />
  }

  return <LoginPage />
}

export default App
