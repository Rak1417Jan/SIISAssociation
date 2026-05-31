import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { CheckCircle, Star, Zap, ArrowRight, ChevronRight, Gift, ShieldCheck, ArrowLeft } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMembershipPlans } from '../services/paymentService';

const PLAN_ICONS = { 'PLAN-001': Star, 'PLAN-002': Zap };
const PLAN_STYLES = {
  'PLAN-001': { color: 'hover:border-blue-500', iconColor: 'bg-blue-50 text-blue-600', badge: 'Popular Choice', badgeColor: 'bg-blue-100 text-blue-700' },
  'PLAN-002': { color: 'hover:border-purple-500', iconColor: 'bg-purple-50 text-purple-600', badge: 'Best Value', badgeColor: 'bg-purple-100 text-purple-700' },
};

export default function PlanSelection() {
  const [selected, setSelected] = useState(null);
  const navigate = useNavigate();
  const { execute: fetchPlans, data: plansRes, loading } = useApi(getMembershipPlans);

  useEffect(() => { fetchPlans(); }, []);

  const plans = plansRes?.data || [];

  const handleSelection = () => {
    const plan = plans.find(p => p.id === selected);
    if (!plan) { toast.error('Please select a plan'); return; }
    localStorage.setItem('amms_selected_plan', JSON.stringify(plan));
    navigate('/payment-summary');
  };

  return (
    <div className="min-h-screen w-full bg-[#f8faff] overflow-hidden relative flex items-center justify-center font-['Plus_Jakarta_Sans',sans-serif] py-20">
      {/* ── Background Elements ────────────────────────── */}
      <div className="absolute inset-0 bg-[linear-gradient(to_right,#e5e7eb_1px,transparent_1px),linear-gradient(to_bottom,#e5e7eb_1px,transparent_1px)] bg-[size:4rem_4rem] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_50%,#000_70%,transparent_100%)] opacity-30" />
      
      {/* Blurry Orbs */}
      <div className="absolute top-[15%] left-[5%] w-96 h-96 bg-blue-400/10 rounded-full blur-[100px] animate-pulse" />
      <div className="absolute top-[40%] right-[10%] w-72 h-72 bg-purple-400/10 rounded-full blur-[90px] animate-pulse" />

      <motion.div 
        initial={{ opacity: 0, y: 30 }} 
        animate={{ opacity: 1, y: 0 }} 
        className="w-full max-w-5xl px-6 relative z-10"
      >
        <button 
          onClick={() => navigate(-1)} 
          className="absolute -top-12 left-6 flex items-center gap-2 px-4 py-2 bg-white rounded-xl shadow-sm border border-slate-100 text-[10px] font-black text-slate-400 hover:text-blue-600 uppercase tracking-widest transition-all hover:-translate-x-1"
        >
           <ArrowLeft size={14} /> Back
        </button>
        <div className="text-center mb-16">
          <div className="w-16 h-16 bg-[#3b82f6] rounded-[20px] flex items-center justify-center mx-auto mb-8 shadow-xl shadow-blue-500/20">
            <Gift className="text-white" size={32} />
          </div>
          <h1 className="text-4xl lg:text-5xl font-[900] text-slate-900 tracking-tight mb-4">Select Your Plan</h1>
          <p className="text-slate-400 font-bold text-sm max-w-md mx-auto">
            Choose a membership tier that aligns with your business goals and industry vision.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-stretch mb-12">
          {loading ? (
            [0,1].map(i => (
              <div key={i} className="bg-white rounded-[48px] p-10 lg:p-14 border-2 border-slate-50 animate-pulse flex flex-col gap-6">
                <div className="w-14 h-14 rounded-2xl bg-slate-100" />
                <div className="h-6 bg-slate-100 rounded-xl w-2/3" />
                <div className="h-10 bg-slate-100 rounded-xl w-1/2" />
                <div className="space-y-3">{[0,1,2,3].map(j => <div key={j} className="h-4 bg-slate-50 rounded-lg" />)}</div>
              </div>
            ))
          ) : plans.map((plan, i) => {
            const Icon = PLAN_ICONS[plan.id] || Star;
            const style = PLAN_STYLES[plan.id] || PLAN_STYLES['PLAN-001'];
            return (
            <motion.div
              key={plan.id}
              initial={{ opacity: 0, x: i === 0 ? -30 : 30 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2 + i * 0.1 }}
              onClick={() => setSelected(plan.id)}
              className={`group cursor-pointer bg-white rounded-[48px] p-10 lg:p-14 border-2 transition-all duration-500 relative flex flex-col ${
                selected === plan.id
                  ? 'border-blue-600 shadow-[0_40px_80px_-20px_rgba(37,99,235,0.15)] scale-[1.02]'
                  : `border-slate-50 ${style.color}`
              }`}
            >
              {selected === plan.id && (
                <div className="absolute -top-4 left-1/2 -translate-x-1/2 bg-blue-600 text-white text-[10px] font-black uppercase tracking-[0.2em] px-6 py-2 rounded-full shadow-lg">
                  Currently Selected
                </div>
              )}
              <div className="flex items-center justify-between mb-8">
                <div className={`w-14 h-14 rounded-2xl flex items-center justify-center ${style.iconColor}`}>
                  <Icon size={28} />
                </div>
                {style.badge && (
                  <span className={`text-[10px] font-black uppercase tracking-widest px-4 py-1.5 rounded-full ${style.badgeColor}`}>
                    {style.badge}
                  </span>
                )}
              </div>
              <div className="mb-8">
                <h3 className="text-2xl font-[900] text-slate-900 tracking-tight mb-4 group-hover:text-blue-600 transition-colors">{plan.name}</h3>
                <div className="flex items-baseline gap-2">
                  <span className="text-sm font-black text-slate-400">₹</span>
                  <span className="text-5xl font-[900] text-slate-900 tracking-tighter">{plan.price.toLocaleString()}</span>
                  <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">{plan.duration ? `PER ${plan.duration} MO` : 'ONE TIME'}</span>
                </div>
              </div>
              <div className="space-y-4 mb-12 flex-1">
                {plan.features.map(f => (
                  <div key={f} className="flex items-center gap-3">
                    <div className={`w-5 h-5 rounded-full flex items-center justify-center ${selected === plan.id ? 'bg-blue-600' : 'bg-slate-100'}`}>
                      <CheckCircle size={12} className={selected === plan.id ? 'text-white' : 'text-slate-400'} />
                    </div>
                    <span className={`text-sm font-bold ${selected === plan.id ? 'text-slate-700' : 'text-slate-400'}`}>{f}</span>
                  </div>
                ))}
              </div>
              <div className={`w-full h-1 relative overflow-hidden rounded-full ${selected === plan.id ? 'bg-blue-600/10' : 'bg-slate-50'}`}>
                {selected === plan.id && (
                  <motion.div initial={{ x: '-100%' }} animate={{ x: '0%' }} className="absolute inset-0 bg-blue-600" />
                )}
              </div>
            </motion.div>
          )})}
        </div>

        <div className="flex flex-col items-center gap-8">
          <Button 
            className="w-full max-w-sm py-6 rounded-[24px] bg-blue-600 hover:bg-blue-700 text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/20 flex items-center justify-center gap-4 group"
            onClick={handleSelection}
          >
            Finalize Selection
            <ChevronRight size={20} className="group-hover:translate-x-1 transition-transform" />
          </Button>

          <div className="flex items-center gap-4 px-8 py-3 bg-white/50 backdrop-blur-md rounded-2xl border border-white">
            <ShieldCheck className="text-green-500" size={18} />
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
              Secure SSL Encryption • PCI DSS Compliance • 256-bit Protection
            </p>
          </div>
        </div>
      </motion.div>
    </div>
  );
}
