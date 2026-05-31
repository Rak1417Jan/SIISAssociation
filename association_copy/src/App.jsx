import React from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AnimatePresence } from 'framer-motion';
import { Toaster } from 'react-hot-toast';

// Layouts
import MainLayout from './layouts/MainLayout';

// Auth
import ProtectedRoute from './components/ProtectedRoute';

// Pages
import LandingPage from './pages/LandingPage';
import Login from './pages/Login';
import MemberLogin from './pages/MemberLogin';
import OTPVerify from './pages/OTPVerify';
import Registration from './pages/Registration';
import PlanSelection from './pages/PlanSelection';
import PaymentSummary from './pages/PaymentSummary';
import PaymentSuccess from './pages/PaymentSuccess';
import MemberDashboard from './pages/MemberDashboard';
import MyInfo from './pages/MyInfo';
import RenewalCenter from './pages/RenewalCenter';
import PaymentHistory from './pages/PaymentHistory';
import StatusTracker from './pages/StatusTracker';
import DigitalIDCard from './pages/DigitalIDCard';
import AdminDashboard from './pages/AdminDashboard';
import MembersList from './pages/MembersList';
import MemberDetail from './pages/MemberDetail';
import FirmsList from './pages/FirmsList';
import FirmEditor from './pages/FirmEditor';
import StaffManagement from './pages/StaffManagement';
import RoleMatrix from './pages/RoleMatrix';
import BroadcastCenter from './pages/BroadcastCenter';
import BusinessAnalytics from './pages/BusinessAnalytics';
import AuditLogs from './pages/AuditLogs';
import PasswordReset from './pages/PasswordReset';
import MasterSettings from './pages/MasterSettings';
import SupportLookup from './pages/SupportLookup';
import MemberDirectory from './pages/MemberDirectory';
import EventManager from './pages/EventManager';
import ReferralSystem from './pages/ReferralSystem';
import GrievancePortal from './pages/GrievancePortal';
import MemberInbox from './pages/MemberInbox';

// Role shorthand arrays
const MEMBER_PLUS  = ['member', 'operator', 'finance', 'admin', 'super_admin'];
const ADMIN_PLUS   = ['admin', 'super_admin'];
const SUPER_ONLY   = ['super_admin'];
const FINANCE_ADMIN = ['finance', 'admin', 'super_admin'];
const OPERATOR_ADMIN = ['operator', 'admin', 'super_admin'];

export default function App() {
  const location = useLocation();

  return (
    <>
      <Toaster position="top-right" gutter={8} />
      
      <AnimatePresence mode="wait">
        <Routes location={location} key={location.pathname}>
          {/* Public / Global Routes */}
          <Route path="/" element={<LandingPage />} />
          <Route path="/landing" element={<LandingPage />} />
          <Route path="/login" element={<Login />} />
          <Route path="/member-login" element={<MemberLogin />} />
          <Route path="/verify" element={<OTPVerify />} />
          <Route path="/register" element={<Registration />} />
          <Route path="/reset-password" element={<PasswordReset />} />

          {/* Main App Routes (Protected via Layout) */}
          <Route element={<MainLayout />}>
            {/* Member Routes — accessible to member and above */}
            <Route path="/dashboard" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><MemberDashboard /></ProtectedRoute>
            } />
            <Route path="/plan-selection" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><PlanSelection /></ProtectedRoute>
            } />
            <Route path="/payment-summary" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><PaymentSummary /></ProtectedRoute>
            } />
            <Route path="/payment-success" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><PaymentSuccess /></ProtectedRoute>
            } />
            <Route path="/my-info" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><MyInfo /></ProtectedRoute>
            } />
            <Route path="/renewal" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><RenewalCenter /></ProtectedRoute>
            } />
            <Route path="/payments" element={
              <ProtectedRoute allowedRoles={['member', 'finance', ...ADMIN_PLUS]}><PaymentHistory /></ProtectedRoute>
            } />
            <Route path="/status" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><StatusTracker /></ProtectedRoute>
            } />
            <Route path="/id-card" element={
              <ProtectedRoute allowedRoles={['member', ...ADMIN_PLUS]}><DigitalIDCard /></ProtectedRoute>
            } />
            <Route path="/inbox" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><MemberInbox /></ProtectedRoute>
            } />
            <Route path="/grievance" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><GrievancePortal /></ProtectedRoute>
            } />
            
            {/* Admin Routes */}
            <Route path="/admin" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><AdminDashboard /></ProtectedRoute>
            } />
            <Route path="/members" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><MembersList /></ProtectedRoute>
            } />
            <Route path="/member/:id" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><MemberDetail /></ProtectedRoute>
            } />
            <Route path="/firms" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><FirmsList /></ProtectedRoute>
            } />
            <Route path="/firm-editor" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><FirmEditor /></ProtectedRoute>
            } />

            {/* Staff Routes */}
            <Route path="/staff" element={
              <ProtectedRoute allowedRoles={SUPER_ONLY}><StaffManagement /></ProtectedRoute>
            } />
            <Route path="/roles" element={
              <ProtectedRoute allowedRoles={SUPER_ONLY}><RoleMatrix /></ProtectedRoute>
            } />
            <Route path="/broadcast" element={
              <ProtectedRoute allowedRoles={ADMIN_PLUS}><BroadcastCenter /></ProtectedRoute>
            } />
            <Route path="/analytics" element={
              <ProtectedRoute allowedRoles={FINANCE_ADMIN}><BusinessAnalytics /></ProtectedRoute>
            } />
            <Route path="/audit" element={
              <ProtectedRoute allowedRoles={SUPER_ONLY}><AuditLogs /></ProtectedRoute>
            } />
            <Route path="/settings" element={
              <ProtectedRoute allowedRoles={SUPER_ONLY}><MasterSettings /></ProtectedRoute>
            } />
            
            {/* Engagement */}
            <Route path="/support" element={
              <ProtectedRoute allowedRoles={OPERATOR_ADMIN}><SupportLookup /></ProtectedRoute>
            } />
            <Route path="/directory" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><MemberDirectory /></ProtectedRoute>
            } />
            <Route path="/events" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><EventManager /></ProtectedRoute>
            } />
            <Route path="/referral" element={
              <ProtectedRoute allowedRoles={MEMBER_PLUS}><ReferralSystem /></ProtectedRoute>
            } />
          </Route>

          {/* Catch all */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AnimatePresence>
    </>
  );
}
