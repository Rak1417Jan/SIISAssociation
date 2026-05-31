import React from 'react';
import { motion } from 'framer-motion';
import { 
  User, 
  CreditCard, 
  Clock, 
  CheckCircle, 
  FileText, 
  Bell, 
  Star, 
  ArrowRight,
  ChevronRight,
  LayoutDashboard,
  ShieldCheck,
  Zap,
  Globe
} from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import { useApi } from '../hooks/useApi';
import { getMemberApplicationStatus } from '../services/analyticsService';
import { getPaymentSummary } from '../services/paymentService';

export default function MemberDashboard() {
  const navigate = useNavigate();
  const userRole = localStorage.getItem('userRole') || 'member';
  const isAdmin = userRole === 'admin';
  const userName = isAdmin ? 'Ravi' : 'Daksh';
  const accountId = isAdmin ? 'VIA-BK-0001' : 'VIA-BK-8842';
  
  const { execute: fetchStatus, data: statusData, loading: statusLoading } = useApi(getMemberApplicationStatus);
  const { execute: fetchPayment, data: paymentData } = useApi(getPaymentSummary);

  React.useEffect(() => {
    fetchStatus('CURRENT_USER');
    fetchPayment('CURRENT_USER');
    const interval = setInterval(() => {
      fetchStatus('CURRENT_USER');
    }, 30000);
    return () => clearInterval(interval);
  }, [fetchStatus, fetchPayment]);

  const appStatus = statusData?.data;
  const paySum = paymentData?.data;
  
  const tierLevel = isAdmin ? 'ADMIN' : (paySum?.currentPlan ? paySum.currentPlan.toUpperCase() : 'PENDING');
  const renderStages = appStatus?.steps || stages;
  const progressPercent = appStatus?.progressPercent || (isAdmin ? 100 : 68);

  return (
    <div className="space-y-10">
      {/* Header Segment */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">MEMBER HUB</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Personal institutional control panel</p>
        </div>
        <div className="flex items-center gap-3">
           <div className="text-right hidden sm:block">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Account ID</p>
              <p className="text-sm font-[900] text-slate-900 tracking-tighter">{accountId}</p>
           </div>
           <Badge status={isAdmin ? 'active' : 'pending'} label={isAdmin ? 'System Admin' : 'Pending Approval'} />
        </div>
      </div>

      {/* Hero Welcome Unit */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2">
           <motion.div 
             initial={{ opacity: 0, y: 20 }}
             animate={{ opacity: 1, y: 0 }}
             className="bg-gradient-to-br from-slate-900 to-slate-800 rounded-[40px] p-10 lg:p-14 text-white relative overflow-hidden group shadow-[0_40px_80px_-20px_rgba(15,23,42,0.2)]"
           >
              <div className="absolute top-[-20%] right-[-10%] w-[400px] h-[400px] bg-blue-600/10 blur-[120px] rounded-full group-hover:scale-110 transition-transform duration-1000" />
              <div className="relative z-10 flex flex-col h-full justify-between">
                 <div>
                    <div className="flex items-center gap-3 mb-8">
                       <LayoutDashboard className="text-blue-500" size={24} />
                       <span className="text-[10px] font-black uppercase tracking-[0.3em] text-slate-400">Institutional Dashboard</span>
                    </div>
                    <h2 className="text-5xl font-[900] tracking-tighter mb-4 italic">Morning, {userName}.</h2>
                    <p className="text-slate-400 font-bold text-sm max-w-sm leading-relaxed uppercase tracking-wider">
                       {isAdmin 
                         ? 'Welcome back to your personal hub. Manage your profile and access admin tools from the sidebar.'
                         : 'Your membership application is currently under final institutional review.'}
                    </p>
                 </div>
                 
                 <div className="mt-12 flex items-center gap-8">
                    <div className="flex flex-col">
                       <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Credit Points</span>
                       <span className="text-2xl font-[900] tracking-tighter text-blue-400 italic">{isAdmin ? '∞' : '2,480 PX'}</span>
                    </div>
                    <div className="w-[1px] h-10 bg-white/10" />
                    <div className="flex flex-col">
                       <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Tier Level</span>
                       <span className="text-2xl font-[900] tracking-tighter text-white italic">{tierLevel}</span>
                    </div>
                 </div>
              </div>
           </motion.div>
        </div>

        <div className="lg:col-span-1 glass-panel p-10 flex flex-col justify-between">
           <div>
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-8 italic">Quick Access</h3>
              <div className="space-y-4">
                 {[
                   { icon: User, label: 'Identity', to: '/my-info' },
                   { icon: CreditCard, label: 'Ledger', to: '/payments' },
                   { icon: Clock, label: 'Renewals', to: '/renewal' },
                   { icon: Star, label: 'ID Card', to: '/id-card' },
                 ].map((item, i) => (
                   <button 
                     key={i}
                     onClick={() => navigate(item.to)}
                     className="w-full p-5 bg-slate-50/50 rounded-2xl border border-slate-100 flex items-center justify-between group hover:bg-white hover:shadow-xl hover:border-transparent transition-all"
                   >
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 rounded-xl bg-white flex items-center justify-center text-slate-400 group-hover:text-blue-600 transition-colors shadow-sm">
                           <item.icon size={18} />
                        </div>
                        <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors">{item.label}</span>
                      </div>
                      <ChevronRight size={14} className="text-slate-300 group-hover:text-blue-600 group-hover:translate-x-1 transition-all" />
                   </button>
                 ))}
              </div>
           </div>
        </div>
      </div>

      {/* Application Timeline Unit */}
      <div className="glass-panel p-10 lg:p-14 relative overflow-hidden">
         <div className="absolute top-0 right-0 w-64 h-64 bg-blue-500/5 blur-[100px] rounded-full pointer-events-none" />
         <h3 className="text-xl font-[900] text-slate-900 tracking-tight mb-12">{isAdmin ? 'System Status' : 'Application Lifecycle'}</h3>
         
         <div className="relative">
            <div className="absolute top-5 left-8 right-8 h-[2px] bg-slate-100 z-0">
               <motion.div 
                 className="h-full bg-blue-600"
                 animate={{ width: `${progressPercent}%` }}
                 transition={{ duration: 1.5 }}
               />
            </div>
            <div className="flex justify-between relative z-10">
               {renderStages.map((s, i) => {
                 const status = s.done ? 'completed' : (i > 0 && renderStages[i-1].done && !s.done) ? 'active' : 'pending';
                 return (
                 <div key={i} className="flex flex-col items-center gap-4 relative group">
                    <div className={`w-10 h-10 rounded-full border-4 border-white shadow-xl flex items-center justify-center transition-all duration-500
                       ${status === 'completed' ? 'bg-blue-600 text-white' : 
                         status === 'active' ? 'bg-amber-400 text-white animate-soft-glow' : 
                         'bg-white text-slate-300 border-slate-50'}
                    `}>
                       {status === 'completed' ? <CheckCircle size={18} /> : <span className="text-xs font-black">{i + 1}</span>}
                    </div>
                    <div className="flex flex-col items-center">
                       <span className={`text-[10px] font-black uppercase tracking-widest text-center max-w-[80px] leading-tight
                          ${status === 'pending' ? 'text-slate-300' : 'text-slate-900'}
                       `}>{s.label}</span>
                       {s.date && <span className="text-[8px] font-bold text-slate-400 mt-1">{s.date}</span>}
                    </div>
                 </div>
               )})}
            </div>
         </div>
         
         {!isAdmin && appStatus && (
           <div className="mt-10 p-6 bg-slate-50 rounded-2xl flex items-center justify-between border border-slate-100">
             <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-white rounded-full flex items-center justify-center text-blue-500 shadow-sm"><Clock size={16}/></div>
                <div>
                   <p className="text-xs font-black text-slate-900 uppercase tracking-widest">Estimated Completion</p>
                   <p className="text-[10px] font-bold text-slate-500 mt-1">{appStatus.estimatedDays} days remaining</p>
                </div>
             </div>
             {appStatus.adminNote && (
               <div className="text-right">
                 <p className="text-[9px] font-black text-amber-500 uppercase tracking-widest mb-1">Admin Note</p>
                 <p className="text-xs font-bold text-slate-700 italic max-w-sm">{appStatus.adminNote}</p>
               </div>
             )}
           </div>
         )}
      </div>

      {/* Intelligence & Notices */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-10">
         <div className="glass-panel p-10">
            <div className="flex items-center justify-between mb-8">
               <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">Institutional Alerts</h3>
               <Bell className="text-blue-600 animate-pulse" size={18} />
            </div>
            <div className="space-y-6">
               {[
                 { title: 'Annual General Assembly 2026', date: 'Exp: 15 MAY', type: 'Event' },
                 { title: 'Institutional Compliance Update', date: 'Exp: 30 APR', type: 'Compliance' },
               ].map((n, i) => (
                 <div key={i} onClick={() => navigate(n.type === 'Event' ? '/events' : '/inbox')} className="p-6 bg-slate-50/50 rounded-3xl border border-slate-100 flex items-center justify-between group hover:bg-white transition-all cursor-pointer">
                    <div className="flex items-center gap-5">
                       <div className="w-12 h-12 rounded-2xl bg-white flex items-center justify-center text-slate-300 group-hover:text-amber-500 transition-colors shadow-sm">
                          <ShieldCheck size={20} />
                       </div>
                       <div>
                          <p className="text-xs font-black text-slate-900 uppercase tracking-wider mb-1">{n.title}</p>
                          <p className="text-[10px] font-bold text-slate-400 tracking-widest uppercase">{n.date}</p>
                       </div>
                    </div>
                    <Badge status="pending" label={n.type} />
                 </div>
               ))}
            </div>
         </div>

         <div className="glass-panel p-10 bg-blue-600 text-white overflow-hidden relative">
            <div className="absolute top-[-50%] left-[-20%] w-96 h-96 bg-white/10 blur-[100px] rounded-full" />
            <div className="relative z-10 flex flex-col h-full justify-between">
               <div>
                  <Zap className="text-blue-200 mb-6" size={32} />
                  <h3 className="text-2xl font-[900] tracking-tighter mb-4 italic leading-tight">{isAdmin ? 'ADMIN\nPROFILE' : 'MEMBER\nACCELERATOR'}</h3>
                  <p className="text-blue-100/60 font-bold text-xs leading-relaxed uppercase tracking-wider max-w-[240px]">
                     {isAdmin 
                       ? 'Keep your admin profile up to date. Access your identity card and personal settings.'
                       : 'Complete your digital profile to unlock global network access and elite institutional features.'}
                  </p>
               </div>
               <Button 
                 onClick={() => navigate('/my-info')}
                 className="mt-8 bg-white text-blue-600 hover:bg-blue-50 font-black text-[10px] uppercase tracking-widest py-4 rounded-xl shadow-xl shadow-blue-900/20"
               >
                  Update Profile <ArrowRight size={14} className="ml-2" />
               </Button>
            </div>
         </div>
      </div>
    </div>
  );
}
