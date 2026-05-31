import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { 
  ShieldCheck, 
  Mail, 
  ArrowLeft, 
  ArrowRight,
  ShieldAlert,
  Zap,
  Globe
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import { useApi } from '../hooks/useApi';
import { resetPassword } from '../services/authService';
import toast from 'react-hot-toast';

export default function PasswordReset() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const { execute: doReset, loading } = useApi(resetPassword);

  const handleReset = async () => {
    if (!email) return;
    const res = await doReset(email);
    if (res.error) {
      toast.error('Email not found');
    } else {
      toast.success(res.data?.message || 'Reset link sent to email');
    }
  };

  return (
    <div className="min-h-screen bg-[#f8fafc] p-10 flex flex-col items-center justify-center relative overflow-hidden">
      {/* Background Orbs & Grid */}
      <div className="absolute inset-0 z-0">
        <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-blue-500/5 blur-[120px] rounded-full" />
        <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-purple-500/5 blur-[120px] rounded-full" />
        <div className="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-20 brightness-100 pointer-events-none" />
      </div>

      <div className="relative z-10 w-full max-w-lg">
         {/* Internal Shell */}
         <div className="glass-panel p-10 lg:p-14 shadow-[0_50px_100px_-20px_rgba(0,0,0,0.05)] border-slate-100/50">
            {/* Header Identity */}
            <div className="flex flex-col items-center mb-12">
               <div className="w-16 h-16 bg-blue-600 rounded-[28px] flex items-center justify-center text-white shadow-xl shadow-blue-500/20 mb-8">
                  <ShieldCheck size={32} />
               </div>
               <h1 className="text-3xl font-[900] text-slate-900 tracking-tighter italic uppercase text-center">Identity Recovery</h1>
               <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-2 text-center max-w-xs">
                  Institutional credential restoration protocol
               </p>
            </div>

            {/* Input Unit */}
            <div className="space-y-8">
               <div className="space-y-2">
                  <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Institutional Email</label>
                  <div className="relative group">
                     <Mail className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within:text-blue-600 transition-colors" size={18} />
                     <input 
                        type="email" 
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="yourname@viatech.org"
                        className="w-full pl-16 pr-8 py-5 bg-slate-50/50 border border-slate-100 rounded-[24px] text-sm font-black outline-none focus:border-blue-500 focus:bg-white transition-all shadow-inner"
                     />
                  </div>
               </div>

               <div className="p-6 bg-amber-50 border border-amber-100 rounded-3xl flex gap-4">
                  <ShieldAlert className="text-amber-500 shrink-0" size={18} />
                  <p className="text-[10px] font-bold text-amber-600 uppercase tracking-wider leading-relaxed">
                     A secure restoration token will be synchronized with your registered mobile device upon request.
                  </p>
               </div>

               <Button 
                 onClick={handleReset}
                 disabled={loading}
                 className="w-full py-6 rounded-[24px] text-sm font-[900] uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/20 flex items-center justify-center gap-3"
               >
                 {loading ? (
                    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                 ) : (
                    <>Initiate Recovery <ArrowRight size={18} /></>
                 )}
               </Button>
            </div>

            {/* Back Context */}
            <div className="mt-12 flex justify-center text-center">
               <button 
                 onClick={() => navigate('/login')}
                 className="flex items-center gap-3 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-[0.2em] transition-colors group"
               >
                  <div className="w-8 h-8 bg-slate-50 rounded-xl flex items-center justify-center group-hover:bg-white group-hover:shadow-lg transition-all">
                    <ArrowLeft size={14} />
                  </div>
                  Return to Matrix
               </button>
            </div>
         </div>

         {/* Bottom Global Indicator */}
         <div className="mt-12 flex flex-col items-center gap-4">
            <div className="flex items-center gap-4">
               <div className="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse" />
               <p className="text-[9px] font-black text-slate-300 uppercase tracking-[0.3em]">Institutional Node Secure</p>
            </div>
            <div className="h-[1px] w-12 bg-slate-200" />
            <div className="flex items-center gap-2">
               <span className="text-[9px] font-[900] text-slate-900 italic tracking-tighter">VIA</span>
               <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">ASSOCIATION</span>
            </div>
         </div>
      </div>
    </div>
  );
}
