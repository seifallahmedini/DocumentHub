import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { SignUpForm } from './auth/SignUpForm'

function Dashboard() {
  return <h1>Dashboard</h1>
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/register" element={<SignUpForm />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="*" element={<Navigate to="/register" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
