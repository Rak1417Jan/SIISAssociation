import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Mail, Lock, LogIn, Users, Building2, ShieldCheck, Eye } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import InteractiveBackground from '../components/ui/InteractiveBackground';
import { useApi } from '../hooks/useApi';
import { adminLogin } from '../services/authService';
import toast from 'react-hot-toast';

export default function Login() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('admin');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { execute: doLogin, loading } = useApi(adminLogin);

  const handleLogin = async () => {
    if (!email || !password) return;
    const res = await doLogin(email, password);
    if (res.error) {
      toast.error('Invalid credentials');
    } else {
      localStorage.setItem('token', res.data.token);
      localStorage.setItem('userRole', 'admin');
      navigate('/admin');
    }
  };

  return (
    <div className="relative min-h-screen bg-white overflow-hidden selection:bg-blue-100">
      {/* 
          OVERRIDE: Exact Grid Background from Image 
          Using a CSS-based grid with the specific blue tint 
      */}
      <div className="absolute inset-0 z-0 opacity-[0.6]" 
        style={{ 
          backgroundImage: `
            linear-gradient(to right, #eff6ff 1.5px, transparent 1.5px),
            linear-gradient(to bottom, #eff6ff 1.5px, transparent 1.5px)
          `,
          backgroundSize: '80px 80px'
        }} 
      />
      
      {/* 3D Atmosphere Layer */}
      <InteractiveBackground />

      <div className="relative z-10 flex min-h-screen items-center justify-center lg:justify-between px-10 lg:px-32 max-w-7xl mx-auto">
        
        {/* LEFT COLUMN: IDENTITIES */}
        <div className="hidden lg:flex flex-col space-y-8 xl:space-y-12 max-w-2xl">
           {/* VIA Logo Box */}
           <motion.div 
             initial={{ opacity: 0, scale: 0.8 }}
             animate={{ opacity: 1, scale: 1 }}
             className="w-16 h-16 xl:w-20 xl:h-20 bg-blue-600 rounded-[22px] flex items-center justify-center shadow-2xl shadow-blue-600/30 ring-8 ring-blue-500/5"
           >
              <span className="text-xl xl:text-2xl font-black text-white tracking-tighter">VIA</span>
           </motion.div>

           <div className="space-y-2 xl:space-y-4">
              <motion.h1 
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.1 }}
                className="text-5xl xl:text-7xl font-[1000] text-[#0f172a] tracking-tighter leading-[0.9] uppercase"
              >
                TEST INDUSTRIES
              </motion.h1>
              <motion.h1 
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.2 }}
                className="text-5xl xl:text-7xl font-[1000] tracking-tighter leading-[0.9] uppercase bg-gradient-to-r from-blue-700 via-indigo-600 to-violet-600 text-transparent bg-clip-text"
              >
                ASSOCIATION
              </motion.h1>
              <motion.p 
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.3 }}
                className="text-lg xl:text-xl font-bold text-slate-400 max-w-sm leading-snug pt-4"
              >
                 Web-Based Member Management & Communication System
              </motion.p>
           </div>

           {/* Stats Cards Row */}
           <div className="flex gap-4 pt-6 xl:pt-10">
              {[
                { icon: Users, val: '500+', label: 'Members' },
                { icon: Building2, val: '120+', label: 'Firms' },
                { icon: ShieldCheck, val: '100%', label: 'Secure' },
              ].map((stat, i) => (
                <motion.div 
                  key={i}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.4 + i * 0.1 }}
                  className="bg-white p-5 xl:p-6 rounded-[28px] shadow-[0_20px_40px_-20px_rgba(0,0,0,0.1)] border border-slate-50 flex flex-col items-center min-w-[110px] xl:min-w-[130px]"
                >
                   <stat.icon size={22} className="text-blue-500 mb-2 xl:mb-3" />
                   <p className="text-xl xl:text-2xl font-black text-slate-900 tracking-tighter italic leading-none mb-1">{stat.val}</p>
                   <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{stat.label}</p>
                </motion.div>
              ))}
           </div>
        </div>

        {/* RIGHT COLUMN: LOGIN CARD */}
        <motion.div 
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          className="w-full max-w-[480px] xl:max-w-[520px] bg-white p-10 xl:p-16 rounded-[48px] shadow-[0_80px_160px_-40px_rgba(0,0,0,0.12)] border border-white/50 relative overflow-hidden"
        >
          {/* Tab Switcher */}
          <div className="mb-8 xl:mb-12 flex bg-slate-50 p-1.5 rounded-[22px]">
             <button 
               onClick={() => setActiveTab('admin')}
               className={`flex-1 py-3.5 xl:py-4 px-4 rounded-[18px] text-[10px] xl:text-[11px] font-black uppercase tracking-widest transition-all
                  ${activeTab === 'admin' ? 'bg-blue-600 text-white shadow-xl shadow-blue-600/20' : 'text-slate-400 hover:text-slate-600'}
               `}
             >
                Admin Login
             </button>
             <button 
               onClick={() => navigate('/member-login')}
               className="flex-1 py-3.5 xl:py-4 px-4 rounded-[18px] text-[10px] xl:text-[11px] font-black uppercase tracking-widest text-slate-400 hover:text-slate-600 transition-all"
             >
                Member Login
             </button>
          </div>

          <div className="space-y-8 xl:space-y-10">
             <div>
                <h2 className="text-3xl xl:text-4xl font-[900] text-slate-900 tracking-tight mb-2">Welcome back</h2>
                <p className="text-[10px] xl:text-[11px] font-black text-slate-400 uppercase tracking-widest">Sign in to your admin control panel</p>
             </div>

             <div className="space-y-4 xl:space-y-5">
                <div className="relative group">
                   <div className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within:text-blue-600 transition-colors">
                      <Mail size={18} />
                   </div>
                   <input 
                      type="text" 
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="Email / Username"
                      className="w-full pl-16 pr-8 py-4 xl:py-5 bg-white border-2 border-slate-50 rounded-[20px] text-sm font-black outline-none focus:border-blue-500/20 focus:ring-4 focus:ring-blue-500/5 transition-all text-slate-900 placeholder:text-slate-300"
                   />
                </div>

                <div className="relative group">
                   <div className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within:text-blue-600 transition-colors">
                      <Lock size={18} />
                   </div>
                   <input 
                      type="password" 
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      placeholder="Password"
                      className="w-full pl-16 pr-14 py-4 xl:py-5 bg-white border-2 border-slate-50 rounded-[20px] text-sm font-black outline-none focus:border-blue-500/20 focus:ring-4 focus:ring-blue-500/5 transition-all text-slate-900 placeholder:text-slate-300"
                   />
                   <button className="absolute right-6 top-1/2 -translate-y-1/2 text-slate-300 hover:text-slate-900">
                      <Eye size={18} />
                   </button>
                </div>

                <div className="flex items-center gap-3 px-1 pt-2">
                   <input type="checkbox" className="w-4 h-4 rounded border-2 border-slate-200 text-blue-600 focus:ring-blue-500" id="remember" />
                   <label htmlFor="remember" className="text-[10px] xl:text-[11px] font-black text-slate-400 uppercase tracking-widest cursor-pointer select-none">Remember Me</label>
                </div>
             </div>

             <Button 
               onClick={handleLogin}
               disabled={loading}
               className="w-full py-5 xl:py-6 rounded-[24px] bg-blue-600 hover:bg-blue-700 text-white font-black text-sm uppercase tracking-widest flex items-center justify-center gap-3 shadow-2xl shadow-blue-600/20 h-auto"
             >
                {loading ? (
                  <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <>
                    <LogIn size={20} />
                    Authorize Access
                  </>
                )}
             </Button>

             <div className="flex flex-col items-center gap-10">
                <button onClick={() => navigate('/reset-password')} className="text-[11px] font-black text-slate-400 hover:text-blue-600 uppercase tracking-widest transition-colors">
                   Forgot Password?
                </button>
                <p className="text-[10px] font-black text-slate-300 uppercase tracking-[0.2em]">
                   © 2026 TEST Industries Association
                </p>
             </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
}
