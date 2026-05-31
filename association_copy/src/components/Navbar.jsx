import React from 'react';
import { motion } from 'framer-motion';
import { Bell, Search, User, Grid, Phone, Settings, Globe } from 'lucide-react';
import { useScrollY } from '../hooks/useScrollY';
import { useSidebar } from '../hooks/useSidebar';
import { useNavigate } from 'react-router-dom';

export default function Navbar() {
  const scrollY = useScrollY();
  const { collapsed } = useSidebar();
  const navigate = useNavigate();

  const userRole = localStorage.getItem('userRole') || 'member';
  const userName = userRole === 'admin' ? 'Ravi Kumar' : 'Daksh Sharma';
  const userInitials = userRole === 'admin' ? 'RK' : 'DS';
  const userTitle = userRole === 'admin' ? 'Executive Admin' : 'Premium Member';

  return (
    <nav className={`fixed top-0 right-0 h-24 z-40 transition-all duration-500 flex items-center px-10 ${collapsed ? 'left-20' : 'left-[280px]'}`}>
      <div className={`w-full h-16 flex items-center justify-between px-8 rounded-[24px] border transition-all duration-300 ${
        scrollY > 20 
          ? 'bg-white/70 backdrop-blur-2xl border-slate-200/50 shadow-xl shadow-slate-200/40 translate-y-2' 
          : 'bg-transparent border-transparent'
      }`}>
         <div className="flex items-center gap-6">
            <div className="flex flex-col">
              <h2 className="text-xl font-black text-slate-900 tracking-tight leading-none uppercase italic">Dashboard</h2>
              <div className="flex items-center gap-2 mt-1">
                <div className="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse" />
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">System Online</span>
              </div>
            </div>
         </div>

         <div className="flex items-center gap-4 sm:gap-8">
            {/* Search Bar */}
            <div className="hidden md:flex items-center relative group">
               <Search size={18} className="absolute left-4 text-slate-400 group-focus-within:text-blue-600 transition-colors" />
               <input 
                 placeholder="Search platform..." 
                 className="bg-slate-100/50 border border-transparent rounded-2xl py-2.5 pl-12 pr-6 text-xs font-bold text-slate-800 placeholder:text-slate-400 focus:bg-white focus:border-blue-500/20 focus:ring-4 focus:ring-blue-500/5 w-72 transition-all duration-300"
               />
               <kbd className="absolute right-4 px-1.5 py-0.5 rounded-md border border-slate-200 bg-white text-[9px] font-black text-slate-400">⌘K</kbd>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2">
               <div className="flex items-center gap-1">
                 <NavIcon icon={Globe} onClick={() => navigate('/directory')} />
                 <NavIcon icon={Bell} badge onClick={() => navigate(userRole === 'admin' ? '/broadcast' : '/inbox')} />
                 <NavIcon icon={Settings} onClick={() => navigate('/settings')} />
               </div>
               
               <div className="h-8 w-px bg-slate-200/50 mx-4 hidden sm:block" />
               
               <button onClick={() => navigate('/my-info')} className="flex items-center gap-4 p-1.5 pr-4 rounded-2xl hover:bg-white hover:shadow-xl hover:shadow-slate-200 transition-all duration-300 border border-transparent active:scale-95 group">
                  <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${userRole === 'admin' ? 'from-blue-600 to-indigo-700' : 'from-emerald-500 to-teal-700'} text-white flex items-center justify-center font-black text-sm shadow-lg shadow-blue-500/20 group-hover:rotate-6 transition-transform`}>
                    {userInitials}
                  </div>
                  <div className="text-left hidden lg:block">
                     <p className="text-xs font-black text-slate-900 leading-tight">{userName}</p>
                     <p className="text-[10px] font-bold text-slate-400 uppercase tracking-[0.1em]">{userTitle}</p>
                  </div>
               </button>
            </div>
         </div>
      </div>
    </nav>
  );
}

function NavIcon({ icon: Icon, badge, onClick }) {
   return (
      <button onClick={onClick} className="relative w-10 h-10 flex items-center justify-center rounded-xl text-slate-400 hover:text-blue-600 hover:bg-white transition-all duration-300 group">
         <Icon size={20} className="transition-transform group-hover:scale-110" />
         {badge && (
           <span className="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-[#fefefe] group-hover:scale-125 transition-transform" />
         )}
      </button>
   );
}
