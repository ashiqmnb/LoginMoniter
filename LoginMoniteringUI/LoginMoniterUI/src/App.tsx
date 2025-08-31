import { BrowserRouter, Routes, Route } from "react-router-dom";
import AdminDashboard from "./pages/AdminDashboard";
import ProtectedRoute from "./routes/ProtectedRoute";
import HomePage from "./pages/HomePage";
import OtpPage from "./pages/OtpPage";
import LoginAttemptsPage from "./pages/LoginAttemptsPage";
import UserStatusPage from "./pages/UserStatusPage";
import RiskEvaluationsPage from "./pages/RiskEvaluationsPage";
import GeoLocationsPage from "./pages/GeoLocationsPage";
import BlackListPage from "./pages/BlackListPage";
import CaptchaForm from "./pages/CaptchaForm";
import RiskLevelsPage from "./pages/RiskLevelsPage";
// import Login from "./pages/Login";
import OctaLogin from "./pages/OctaLogin";



function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<OctaLogin />} />
        <Route path="/home" element={<HomePage />} />
        <Route path="/verify-otp" element={<OtpPage />} />
        <Route path="/captcha" element={<CaptchaForm />} />


        <Route
          path="/admin"
          element={ 
            <ProtectedRoute role="admin">
              <AdminDashboard />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/login-attempts"
          element={
            <ProtectedRoute role="admin">
              <LoginAttemptsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/user-status"
          element={
            <ProtectedRoute role="admin">
              <UserStatusPage />
            </ProtectedRoute>
          }
        />
``
        <Route
          path="/admin/risk-evaluations"
          element={
            <ProtectedRoute role="admin">
              <RiskEvaluationsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/all-locations"
          element={
            <ProtectedRoute role="admin">
              <GeoLocationsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/blacklist"
          element={
            <ProtectedRoute role="admin">
              <BlackListPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/admin/risk-levels"
          element={
            <ProtectedRoute role="admin">
              <RiskLevelsPage />
            </ProtectedRoute>
          }
        />

      </Routes>
    </BrowserRouter>
  );
}

export default App;
