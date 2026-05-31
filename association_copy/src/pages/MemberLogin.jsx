import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { 
  User, 
  ArrowRight, 
  ShieldCheck, 
  Zap, 
  Globe,
  ArrowLeft,
  Lock
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import InteractiveBackground from '../components/ui/InteractiveBackground';
import { useApi } from '../hooks/useApi';
import { requestOTP } from '../services/authService';
import toast from 'react-hot-toast';

export default function MemberLogin() {
  const navigate = useNavigate();
  const [phone, setPhone] = useState('');
  const { execute: doRequestOTP, loading } = useApi(requestOTP);

  const handleLogin = async () => {
    if (!phone) return;
    const res = await doRequestOTP(phone);
    if (res.error) {
      toast.error('Invalid phone number or not found');
    } else {
      toast.success('OTP sent successfully');
      navigate('/verify', { state: { phone } });
    }
  };

  return (
    <div className="relative min-h-screen bg-[#f8fafc] overflow-hidden selection:bg-blue-100 selection:text-blue-900">
      {/* Premium 3D Background Layer */}
      <InteractiveBackground />

      {/* Main Content Layer */}
      <div className="relative z-10 flex min-h-screen flex-col xl:flex-row">
        {/* Visual Left Blade */}
        <div className="hidden xl:flex w-1/2 p-24 flex-col justify-between relative overflow-hidden">
          <div className="relative z-10">
            <motion.div 
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              className="flex items-center gap-4 mb-24 group cursor-pointer"
              onClick={() => navigate('/')}
            >
              <div className="w-14 h-14 bg-slate-900 rounded-[22px] flex items-center justify-center font-black text-xs text-white shadow-2xl transition-transform group-hover:scale-110">VIA</div>
              <div>
                <h2 className="text-sm font-[900] text-slate-900 tracking-tighter italic">ASSOCIATION</h2>
                <div className="flex items-center gap-2">
                  <div className="w-1.5 h-1.5 rounded-full bg-blue-500 animate-pulse" />
                  <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Member Portal</span>
                </div>
              </div>
            </motion.div>

            <div className="max-w-xl">
              <motion.h1 
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="text-5xl md:text-7xl font-[900] text-slate-900 tracking-tighter leading-[0.9] italic uppercase mb-10"
              >
                Connect to the<br />
                <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-indigo-600">Network.</span>
              </motion.h1>
              <motion.p 
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3 }}
                className="text-lg font-bold text-slate-400 uppercase tracking-widest leading-relaxed max-w-md"
              >
                Access global industry hubs and elite institutional resources from your personal workstation.
              </motion.p>
            </div>
          </div>

          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.5 }}
            className="relative z-10 grid grid-cols-2 gap-10"
          >
            {[
              { icon: Zap, label: 'Transmission', value: '4.2K Nodes Active', color: 'text-amber-500' },
              { icon: ShieldCheck, label: 'Identity', value: 'Verified Sync', color: 'text-green-500' },
            ].map((stat, i) => (
              <div key={i} className="space-y-3">
                <stat.icon className={stat.color} size={28} />
                <div>
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">{stat.label}</p>
                  <p className="text-sm font-[900] text-slate-900 tracking-tighter italic uppercase">{stat.value}</p>
                </div>
              </div>
            ))}
          </motion.div>
        </div>

        {/* Right Section: Identity Shell */}
        <div className="flex-1 flex items-center justify-center p-10 lg:p-20">
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="w-full max-w-[480px] bg-white/80 backdrop-blur-2xl p-10 lg:p-14 rounded-[48px] shadow-[0_80px_160px_-40px_rgba(0,0,0,0.1)] border border-white relative group overflow-hidden"
          >
            <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 blur-3xl rounded-full group-hover:bg-blue-500/10 transition-colors" />
            
            <div className="relative z-10 mb-14 flex items-center justify-between">
               <button onClick={() => navigate('/login')} className="flex items-center gap-3 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-[0.2em] transition-colors group">
                  <ArrowLeft size={16} className="group-hover:-translate-x-1 transition-transform" /> Back to Matrix
               </button>
               <span className="text-[10px] font-black text-blue-600 uppercase tracking-[0.3em] font-mono italic">MEMBER_AUTH</span>
            </div>

            <div className="space-y-10 relative z-10">
              <div className="space-y-3">
                <div className="w-16 h-16 bg-slate-50 rounded-[24px] flex items-center justify-center text-slate-300">
                  <Lock size={28} />
                </div>
                <h2 className="text-3xl font-[900] text-slate-900 tracking-tighter italic uppercase">Member Access</h2>
                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic ml-0.5">Please provide your institutional credentials</p>
              </div>

              <div className="space-y-8">
                <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Registry Email</label>
                  <div className="relative group/input">
                    <User className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within/input:text-blue-600 transition-colors" size={18} />
                    <input 
                      type="text" 
                      value={phone}
                      onChange={(e) => setPhone(e.target.value)}
                      placeholder="Phone Number / Email"
                      className="w-full pl-16 pr-8 py-5 bg-slate-50/50 border border-slate-100 rounded-[24px] text-sm font-black outline-none focus:bg-white focus:border-blue-500 transition-all shadow-inner"
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex justify-between px-1">
                     <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Access Key</label>
                     <button onClick={() => navigate('/reset-password')} className="text-[10px] font-black text-blue-600 uppercase tracking-widest hover:text-blue-700">Restore Access</button>
                  </div>
                  <div className="relative group/input">
                    <ShieldCheck className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within/input:text-blue-600 transition-colors" size={18} />
                    <input 
                      type="password" 
                      placeholder="••••••••••••"
                      className="w-full pl-16 pr-8 py-5 bg-slate-50/50 border border-slate-100 rounded-[24px] text-sm font-black outline-none focus:bg-white focus:border-blue-500 transition-all shadow-inner"
                    />
                  </div>
                </div>

                <Button 
                  onClick={handleLogin}
                  disabled={loading}
                  className="w-full py-6 rounded-[28px] bg-slate-900 hover:bg-blue-600 text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-2xl transition-all h-auto"
                >
                  {loading ? (
                    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin mx-auto" />
                  ) : (
                    <>
                      Confirm Sync <ArrowRight size={18} className="ml-2 inline-block" />
                    </>
                  )}
                </Button>
              </div>

              <div className="pt-10 border-t border-slate-100 flex flex-col items-center gap-6">
                <p className="text-[9px] font-black text-slate-300 uppercase tracking-widest">New to the Association?</p>
                <button 
                  onClick={() => navigate('/register')}
                  className="w-full py-4 rounded-xl border border-slate-200 text-[10px] font-black text-slate-900 uppercase tracking-widest hover:bg-slate-50 transition-all"
                >
                  Apply for Institutional Membership
                </button>
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    </div>
  );
}
