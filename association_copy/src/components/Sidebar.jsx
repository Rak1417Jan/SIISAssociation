import React from 'react';
import { motion } from 'framer-motion';
import { 
  Loader2, 
  LayoutDashboard, 
  Users, 
  Building2, 
  CreditCard, 
  Settings, 
  LogOut, 
  Bell, 
  Search, 
  ShieldCheck,
  ChevronLeft, 
  BarChart3, 
  Radio, 
  FileText, 
  Calendar,
  Gift, 
  Heart, 
  LifeBuoy,
  Zap,
  Lock,
  Inbox
} from 'lucide-react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useSidebar } from '../hooks/useSidebar';
import { useAuth } from '../context/AuthContext';

export default function Sidebar() {
  const { collapsed, toggle } = useSidebar();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const userRole = user?.role || localStorage.getItem('userRole') || 'member';

  const adminMenu = [
    { label: 'Dashboard', icon: LayoutDashboard, path: '/dashboard', cat: 'PERSONAL' },
    { label: 'My Info', icon: ShieldCheck, path: '/my-info' },

    { label: 'Admin Hub', icon: Lock, path: '/admin', cat: 'EXECUTIVE' },
    { label: 'Members', icon: Users, path: '/members' },
    { label: 'Firms', icon: Building2, path: '/firms' },
    { label: 'Analytics', icon: BarChart3, path: '/analytics' },
    
    { label: 'Broadcast', icon: Radio, path: '/broadcast', cat: 'ENGAGEMENT' },
    { label: 'Events', icon: Calendar, path: '/events' },
    { label: 'Audit Logs', icon: FileText, path: '/audit' },
    { label: 'Settings', icon: Settings, path: '/settings', cat: 'SYSTEM' },
    { label: 'Support', icon: LifeBuoy, path: '/support' },
  ];

  const memberMenu = [
    { label: 'Dashboard', icon: LayoutDashboard, path: '/dashboard', cat: 'OVERVIEW' },
    { label: 'My Info', icon: ShieldCheck, path: '/my-info' },
    { label: 'Renewal', icon: Zap, path: '/renewal' },
    { label: 'Inbox', icon: Inbox, path: '/inbox' },
    
    { label: 'Directory', icon: Heart, path: '/directory', cat: 'ASSOCIATION' },
    { label: 'Events', icon: Calendar, path: '/events' },
    { label: 'Support', icon: LifeBuoy, path: '/support', cat: 'HELP' },
    { label: 'Settings', icon: Settings, path: '/settings' },
  ];

  const menu = userRole === 'admin' || userRole === 'super_admin' ? adminMenu : memberMenu;

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className={`fixed left-0 top-0 h-screen z-50 transition-all duration-500 ease-in-out border-r border-slate-100 bg-white/80 backdrop-blur-2xl ${collapsed ? 'w-20' : 'w-[280px]'}`}>
      {/* Brand Section */}
      <div className="h-24 flex items-center px-6">
        <motion.div 
          onClick={() => navigate('/dashboard')}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          className="w-10 h-10 bg-gradient-to-br from-blue-600 to-indigo-700 rounded-xl flex items-center justify-center shadow-lg shadow-blue-500/20 cursor-pointer overflow-hidden shrink-0"
        >
          <span className="text-white font-black text-xs tracking-tighter">VIA</span>
        </motion.div>
        {!collapsed && (
          <motion.div 
            initial={{ opacity: 0, x: -10 }} 
            animate={{ opacity: 1, x: 0 }} 
            className="ml-4"
          >
             <h1 className="text-lg font-black text-slate-900 tracking-tighter leading-none italic">ASSOCIATION</h1>
             <p className="text-[9px] font-black text-slate-400 uppercase tracking-[0.2em] mt-1">Management Cloud</p>
          </motion.div>
        )}
      </div>

      {/* Main Navigation */}
      <div className="px-4 space-y-2 overflow-y-auto max-h-[calc(100vh-180px)] pb-20 scrollbar-hide">
        {menu.map((item, i) => (
          <React.Fragment key={i}>
             {item.cat && !collapsed && (
               <p className="text-[10px] font-black text-slate-300 mt-8 mb-4 ml-3 uppercase tracking-[0.2em]">{item.cat}</p>
             )}
             <NavLink
               to={item.path}
               className={({ isActive }) => `
                 flex items-center gap-3 px-4 py-3 rounded-2xl transition-all duration-300 group relative
                 ${isActive 
                    ? 'bg-blue-600 text-white shadow-xl shadow-blue-500/30' 
                    : 'text-slate-500 hover:bg-slate-50 hover:text-blue-600'}
               `}
             >
                <item.icon size={20} className={`shrink-0 transition-transform duration-300 group-hover:scale-110 ${collapsed ? 'mx-auto' : ''}`} />
                {!collapsed && (
                  <span className="text-sm font-bold tracking-tight">{item.label}</span>
                )}
                
                {/* Tooltip for collapsed mode */}
                {collapsed && (
                  <div className="absolute left-full ml-4 px-3 py-2 bg-slate-900 text-white text-[10px] font-bold rounded-xl opacity-0 group-hover:opacity-100 pointer-events-none transition-all translate-x-2 group-hover:translate-x-0 whitespace-nowrap z-50">
                    {item.label}
                  </div>
                )}
             </NavLink>
          </React.Fragment>
        ))}
      </div>

      {/* Account / Action Section */}
      <div className="absolute bottom-6 left-0 w-full px-4">
        <motion.button 
          whileHover={{ x: 4 }}
          onClick={handleLogout}
          className={`w-full flex items-center gap-3 px-4 py-4 rounded-2xl text-slate-400 hover:text-red-500 hover:bg-red-50/50 transition-all font-black text-xs uppercase tracking-widest ${collapsed ? 'justify-center' : ''}`}
        >
          <LogOut size={20} className="shrink-0" />
          {!collapsed && <span>System Logout</span>}
        </motion.button>
      </div>

      {/* Toggle Controls */}
      <button 
        onClick={toggle}
        className="absolute -right-3.5 top-10 w-7 h-7 rounded-full bg-white border border-slate-100 shadow-md flex items-center justify-center text-slate-400 hover:text-blue-600 hover:border-blue-100 transition-all z-50"
      >
        <ChevronLeft size={16} className={`transition-transform duration-500 ${collapsed ? 'rotate-180' : ''}`} />
      </button>
    </aside>
  );
}
