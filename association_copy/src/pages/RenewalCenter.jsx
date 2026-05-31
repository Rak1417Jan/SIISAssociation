import React, { useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Calendar, 
  ArrowRight, 
  ShieldCheck, 
  Zap, 
  CreditCard,
  History,
  CheckCircle,
  AlertTriangle
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getPaymentSummary, processRenewal, verifyPayment } from '../services/paymentService';
import { openRazorpayModal } from '../services/razorpayService';

export default function RenewalCenter() {
  const navigate = useNavigate();
  const { execute: fetchSummary, data: summaryRes, loading } = useApi(getPaymentSummary);
  const { execute: doRenewal } = useApi(processRenewal);
  const { execute: doVerify } = useApi(verifyPayment);

  useEffect(() => { fetchSummary('CURRENT_USER'); }, []);

  const summary = summaryRes?.data;
  const isUrgent = summary?.daysUntilExpiry != null && summary.daysUntilExpiry < 30;

  const handleRenewNow = async () => {
    const tid = toast.loading('Preparing renewal...');
    const orderRes = await doRenewal('CURRENT_USER', 'PLAN-001');
    if (orderRes.error) { toast.error('Could not initiate renewal', {id: tid}); return; }
    toast.dismiss(tid);
    openRazorpayModal(orderRes.data,
      async (resp) => {
        const vr = await doVerify(resp.razorpayOrderId, resp.razorpayPaymentId, resp.razorpaySignature);
        if (!vr.error) { toast.success('Renewal successful!'); navigate('/payment-success', { state: { payment: vr.data } }); }
        else toast.error('Verification failed.');
      },
      (msg) => toast.error(msg || 'Renewal cancelled')
    );
  };

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">RENEWAL COCKPIT</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Maintain your membership status</p>
        </div>
        <button
          onClick={handleRenewNow}
          className="flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-2xl text-[10px] font-black uppercase tracking-widest shadow-xl shadow-blue-500/20 active:scale-95 transition-all"
        >
          <CreditCard size={14} /> Renew Now
        </button>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
         {/* Main Renewal Control */}
         <div className="xl:col-span-2 space-y-8">
            <div className="glass-panel p-10 lg:p-14 relative overflow-hidden group">
               <div className="absolute top-0 right-0 w-80 h-80 bg-blue-500/5 blur-[100px] rounded-full pointer-events-none" />
               <div className="relative z-10">
                  <div className="flex items-start justify-between mb-12">
                     <div className="w-16 h-16 bg-blue-50 rounded-[28px] flex items-center justify-center text-blue-600 shadow-inner">
                        <Calendar size={32} />
                     </div>
                      <div className="text-right">
                         <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Membership Expiry</p>
                         <h2 className={`text-3xl font-[900] tracking-tighter italic uppercase ${isUrgent ? 'text-red-500' : 'text-slate-900'}`}>
                           {loading ? '...' : summary?.daysUntilExpiry != null ? `${summary.daysUntilExpiry} DAYS LEFT` : 'LIFETIME'}
                         </h2>
                      </div>
                  </div>

                  <div className="bg-slate-50/50 rounded-[40px] p-10 border border-slate-100 mb-12">
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-4">Current Status</p>
                      <div className="flex items-center gap-6">
                         <div className={`w-10 h-10 rounded-full flex items-center justify-center text-white ${isUrgent ? 'bg-amber-500 shadow-[0_0_20px_rgba(245,158,11,0.3)]' : 'bg-green-500 shadow-[0_0_20px_rgba(34,197,94,0.3)]'}`}>
                            {isUrgent ? <AlertTriangle size={20} /> : <CheckCircle size={20} />}
                         </div>
                         <div>
                            <h3 className="text-xl font-[900] text-slate-900 tracking-tight">{loading ? 'Loading...' : `Active ${summary?.currentPlan || ''} Plan`}</h3>
                            <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">
                              {loading ? '' : isUrgent ? `⚠ Expires ${summary?.nextRenewalDate}` : `Next Renewal: ${summary?.nextRenewalDate || '—'}`}
                            </p>
                         </div>
                      </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                     <div className="p-8 border-2 border-slate-50 rounded-[32px] hover:border-blue-200 transition-all cursor-pointer group/card">
                        <div className="flex items-center gap-4 mb-4">
                           <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600">
                             <Zap size={20} />
                           </div>
                           <h4 className="text-xs font-black text-slate-900 uppercase tracking-widest">Early Renewal</h4>
                        </div>
                        <p className="text-xs font-bold text-slate-400 leading-relaxed uppercase tracking-wider mb-6">Extend your membership now for 24 months and get an exclusive Founding Elite badge.</p>
                        <button 
                           onClick={() => navigate('/plan-selection')}
                           className="text-[10px] font-black text-blue-600 uppercase tracking-widest flex items-center gap-2 group-hover/card:translate-x-1 transition-transform"
                         >
                            View Offer <ArrowRight size={14} />
                         </button>
                     </div>

                     <div className="p-8 border-2 border-slate-50 rounded-[32px] hover:border-purple-200 transition-all cursor-pointer group/card">
                        <div className="flex items-center gap-4 mb-4">
                           <div className="w-10 h-10 rounded-xl bg-purple-50 flex items-center justify-center text-purple-600">
                             <ShieldCheck size={20} />
                           </div>
                           <h4 className="text-xs font-black text-slate-900 uppercase tracking-widest">Auto-Renew</h4>
                        </div>
                        <p className="text-xs font-bold text-slate-400 leading-relaxed uppercase tracking-wider mb-6">Enable automatic billing to ensure your institutional access is never interrupted.</p>
                        <button 
                           onClick={() => navigate('/settings')}
                           className="text-[10px] font-black text-purple-600 uppercase tracking-widest flex items-center gap-2 group-hover/card:translate-x-1 transition-transform"
                         >
                            Configure <ArrowRight size={14} />
                         </button>
                     </div>
                  </div>
               </div>
            </div>
         </div>

         {/* Sidebar: Details */}
         <div className="xl:col-span-1 space-y-8">
            <div className="glass-panel p-10 bg-slate-900 text-white overflow-hidden relative">
               <div className="absolute top-0 left-0 w-full h-1 bg-blue-600" />
               <h3 className="text-lg font-[900] tracking-tight mb-8 italic">MEMBER BENEFITS</h3>
               <div className="space-y-6">
                  {[
                    'Priority Event Entry',
                    'Digital ID Credentials',
                    'Logistics Network Access',
                    'Global Industry Support',
                    'Elite Dashboard Analytics'
                  ].map((b, i) => (
                    <div key={i} className="flex items-center gap-3">
                       <CheckCircle size={16} className="text-blue-500" />
                       <span className="text-xs font-bold text-slate-400 uppercase tracking-widest">{b}</span>
                    </div>
                  ))}
               </div>
               <div className="mt-12 p-6 bg-white/5 rounded-2xl border border-white/10">
                  <div className="flex items-start gap-4">
                     <AlertTriangle className="text-amber-500 shrink-0" size={18} />
                     <p className="text-[10px] font-bold text-slate-400 leading-relaxed uppercase tracking-wider">
                        Maintain continuous membership to preserve your loyalty discount and historical data access.
                     </p>
                  </div>
               </div>
            </div>

            <div className="glass-panel p-8">
               <div className="flex items-center gap-4 mb-6">
                  <History size={18} className="text-slate-300" />
                  <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">History</h3>
               </div>
               <div className="space-y-4">
                   <div className="flex justify-between items-center py-2 border-b border-slate-50">
                      <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Last Payment</span>
                      <span className="text-[10px] font-black text-slate-900 uppercase tracking-widest">{summary?.lastPayment || '—'}</span>
                   </div>
                   <div className="flex justify-between items-center py-2 border-b border-slate-50">
                      <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Total Paid</span>
                      <span className="text-[10px] font-black text-slate-900 uppercase tracking-widest">₹{summary?.totalPaid?.toLocaleString() || '—'}</span>
                   </div>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
}
