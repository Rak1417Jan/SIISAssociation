import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ShieldAlert, ArrowLeft } from 'lucide-react';

/**
 * ProtectedRoute wraps route elements to enforce authentication and role-based access.
 *
 * @param {React.ReactNode} children - The route component to render
 * @param {string[]} allowedRoles - Roles permitted to access this route
 */
export default function ProtectedRoute({ children, allowedRoles }) {
  const { user, token, isLoading } = useAuth();
  const location = useLocation();

  // Show nothing while validating session
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-white">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  // Not authenticated → redirect to login
  if (!token || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Authenticated but wrong role → show Unauthorized page (not a redirect)
  if (allowedRoles && allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center space-y-6 max-w-md">
          <div className="w-20 h-20 bg-red-50 rounded-[28px] flex items-center justify-center mx-auto">
            <ShieldAlert size={36} className="text-red-500" />
          </div>
          <h1 className="text-3xl font-[900] text-slate-900 tracking-tight italic uppercase">
            Access Restricted
          </h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">
            Your current role ({user.role}) does not have permission to access this module.
          </p>
          <p className="text-xs font-bold text-slate-500">
            Contact your system administrator to request elevated privileges.
          </p>
          <button
            onClick={() => window.history.back()}
            className="inline-flex items-center gap-2 px-8 py-4 bg-slate-900 text-white rounded-2xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-600 transition-colors shadow-xl active:scale-95"
          >
            <ArrowLeft size={14} />
            Return to Safety
          </button>
        </div>
      </div>
    );
  }

  return children;
}
