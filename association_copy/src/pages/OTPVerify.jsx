import React, { useState, useEffect, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useLocation, useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button';
import { 
  ShieldCheck, 
  RefreshCw, 
  ArrowRight,
  ChevronLeft,
  Lock
} from 'lucide-react';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { verifyOTP, requestOTP } from '../services/authService';

export default function OTPVerify() {
  const { state } = useLocation();
  const navigate = useNavigate();
  const phone = state?.phone || '9876543210';
  const [otp, setOtp] = useState(['', '', '', '', '', '']);
  const [timer, setTimer] = useState(60);
  const [error, setError] = useState('');
  const [attempts, setAttempts] = useState(0);
  const { execute: doVerifyOTP, loading: verifying } = useApi(verifyOTP);
  const { execute: doRequestOTP } = useApi(requestOTP);
  const refs = useRef([]);

  useEffect(() => {
    if (timer <= 0) return;
    const t = setTimeout(() => setTimer(v => v - 1), 1000);
    return () => clearTimeout(t);
  }, [timer]);

  const handleChange = (idx, val) => {
    if (!/^\d?$/.test(val)) return;
    const next = [...otp];
    next[idx] = val;
    setOtp(next);
    setError('');
    if (val && idx < 5) refs.current[idx + 1]?.focus();
  };

  const handleKeyDown = (idx, e) => {
    if (e.key === 'Backspace' && !otp[idx] && idx > 0) refs.current[idx - 1]?.focus();
  };

  const handleVerify = async () => {
    if (otp.join('').length < 6) { 
      setError('Please enter the 6-digit OTP'); 
      return; 
    }
    if (attempts >= 3) {
      setError('Too many attempts. Please request a new code.');
      return;
    }
    
    const res = await doVerifyOTP(phone, otp.join(''));
    if (res.error) {
      const newAttempts = attempts + 1;
      setAttempts(newAttempts);
      if (newAttempts >= 3) {
        setError('Too many attempts. Please request a new code.');
      } else {
        setError('Invalid OTP');
      }
    } else {
      toast.success('Identity verified!');
      if (res.data.isNewUser) {
        navigate('/register');
      } else {
        localStorage.setItem('token', res.data.token);
        localStorage.setItem('userRole', res.data.user?.role || 'member');
        navigate('/dashboard');
      }
    }
  };

  return (
    <div className="min-h-screen w-full bg-[#f8faff] overflow-hidden relative flex items-center justify-center font-['Plus_Jakarta_Sans',sans-serif]">
      {/* ── Background Elements ────────────────────────── */}
      <div className="absolute inset-0 bg-[linear-gradient(to_right,#e5e7eb_1px,transparent_1px),linear-gradient(to_bottom,#e5e7eb_1px,transparent_1px)] bg-[size:4rem_4rem] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_50%,#000_70%,transparent_100%)] opacity-30" />
      
      {/* Blurry Orbs */}
      <div className="absolute top-[10%] left-[10%] w-72 h-72 bg-blue-400/10 rounded-full blur-[100px] animate-pulse" />
      <div className="absolute bottom-[15%] right-[5%] w-96 h-96 bg-indigo-400/10 rounded-full blur-[120px] animate-pulse" />

      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.6 }}
        className="w-full max-w-lg px-6 relative z-10"
      >
        <div className="bg-white rounded-[40px] p-10 lg:p-14 shadow-[0_40px_80px_-20px_rgba(59,130,246,0.1)] border border-slate-50 relative overflow-hidden">
          {/* Top Branding */}
          <div className="flex flex-col items-center mb-12 text-center">
            <div className="w-16 h-16 bg-blue-600 rounded-[20px] flex items-center justify-center mb-6 shadow-lg shadow-blue-500/20">
              <ShieldCheck className="text-white" size={32} />
            </div>
            <h1 className="text-3xl font-[900] text-slate-900 tracking-tight mb-2">Security Verification</h1>
            <p className="text-slate-400 font-bold text-sm max-w-[280px]">
              Enter the 6-digit code sent to your registered mobile
            </p>
            <div className="mt-4 px-4 py-2 bg-slate-50 rounded-full text-blue-600 font-black text-xs tracking-widest uppercase">
              +91 {phone.replace(/(\d{3})(\d{3})(\d{4})/, '$1 $2 $3')}
            </div>
          </div>

          <div className="space-y-10">
            {/* OTP Grid */}
            <div className="flex gap-3 justify-center">
              {otp.map((digit, idx) => (
                <input
                  key={idx}
                  ref={el => refs.current[idx] = el}
                  value={digit}
                  onChange={e => handleChange(idx, e.target.value)}
                  onKeyDown={e => handleKeyDown(idx, e)}
                  maxLength={1}
                  className={`w-12 h-16 text-center text-2xl font-[900] rounded-[20px] outline-none
                    transition-all duration-300 border-2
                    ${error ? 'border-red-200 bg-red-50 text-red-600' : 
                      digit ? 'border-blue-600 bg-blue-50 text-blue-600' : 'border-slate-100 bg-slate-50 text-slate-400'}
                    focus:border-blue-600 focus:bg-white focus:ring-4 focus:ring-blue-500/5`}
                />
              ))}
            </div>

            {/* Error Message */}
            <AnimatePresence>
              {error && (
                <motion.div
                  initial={{ opacity: 0, scale: 0.95 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.95 }}
                  className="bg-red-50 text-red-600 p-4 rounded-2xl border border-red-100 text-xs font-black text-center uppercase tracking-widest"
                >
                  {error}
                </motion.div>
              )}
            </AnimatePresence>

            <Button 
              className="w-full py-5 rounded-[22px] bg-blue-600 hover:bg-blue-700 text-white font-black text-sm uppercase tracking-widest flex items-center justify-center gap-3 shadow-xl shadow-blue-500/20"
              onClick={handleVerify} 
              disabled={verifying || attempts >= 3}
            >
              {verifying ? (
                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <>
                  <span>Verify Account</span>
                  <ArrowRight size={20} />
                </>
              )}
            </Button>

            {/* Resend Actions */}
            <div className="flex flex-col items-center gap-6">
              {timer > 0 ? (
                <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400">
                  Resend code in <span className="text-blue-600">{timer}s</span>
                </p>
              ) : (
                <button
                  onClick={async () => {
                    const res = await doRequestOTP(phone);
                    if (!res.error) {
                      setTimer(60);
                      toast.success('New code sent');
                      setAttempts(0);
                      setError('');
                      setOtp(['', '', '', '', '', '']);
                      refs.current[0]?.focus();
                    } else {
                      toast.error('Failed to resend code');
                    }
                  }}
                  className="text-[10px] font-black uppercase tracking-[0.2em] text-blue-600 hover:text-blue-700 flex items-center gap-2"
                >
                  <RefreshCw size={12} />
                  Request New Code
                </button>
              )}

              <div className="pt-8 border-t border-slate-50 w-full flex justify-center">
                <button
                  onClick={() => navigate('/login')}
                  className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-300 hover:text-slate-600 flex items-center gap-2"
                >
                  <ChevronLeft size={16} />
                  Change Phone Number
                </button>
              </div>
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
}
