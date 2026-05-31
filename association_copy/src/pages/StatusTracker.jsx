import React, { useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useLocation } from 'react-router-dom';
import { 
  CheckCircle, 
  Clock, 
  MapPin, 
  FileSearch, 
  ShieldCheck, 
  ChevronRight,
  Package,
  Truck,
  Zap,
  AlertCircle
} from 'lucide-react';
import { Badge } from '../components/ui/Badge';
import { useApi } from '../hooks/useApi';
import { getRegistrationStatus } from '../services/registrationService';

const getStepIcon = (status) => {
  switch (status) {
    case 'APPLIED': return FileSearch;
    case 'DOCUMENT_REVIEW': return ShieldCheck;
    case 'APPROVED': return CheckCircle;
    case 'REJECTED': return AlertCircle;
    default: return Clock;
  }
};

export default function StatusTracker() {
  const { state } = useLocation();
  const appId = state?.applicationId || "APP-2024-001";
  const { execute: fetchStatus, data: statusData, loading } = useApi(getRegistrationStatus);

  useEffect(() => {
    fetchStatus(appId);
    const interval = setInterval(() => {
      fetchStatus(appId);
    }, 30000); 
    // TODO: Replace polling with WebSocket later
    return () => clearInterval(interval);
  }, [appId, fetchStatus]);
  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">ORDER STATUS</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Track your membership deliverables</p>
        </div>
        <div className="flex items-center gap-4">
          <p className="text-[10px] font-black text-slate-300 uppercase tracking-widest hidden sm:block">Reference ID: #{appId}</p>
          <Badge 
             status={statusData?.status?.toLowerCase() || 'pending'} 
             label={statusData?.status || 'LOADING...'} 
          />
        </div>
      </div>

      {statusData?.status === 'REJECTED' && (
        <div className="p-6 bg-red-50 border-l-4 border-red-500 rounded-r-2xl mb-8 flex gap-4 items-start">
          <AlertCircle className="text-red-500 shrink-0" size={24} />
          <div>
            <h3 className="text-red-800 font-[900] text-sm uppercase tracking-widest mb-1">Action Required</h3>
            <p className="text-red-600 font-bold text-xs uppercase tracking-wider">{statusData.adminNotes}</p>
            <button className="mt-4 px-6 py-2 bg-red-600 text-white font-black text-[10px] uppercase tracking-widest rounded-full hover:bg-red-700 transition-colors">
              Fix Application
            </button>
          </div>
        </div>
      )}

      {statusData?.status === 'PENDING' && statusData?.estimatedDays && (
        <div className="p-6 bg-blue-50 border-l-4 border-blue-500 rounded-r-2xl mb-8 flex gap-4 items-start">
          <Clock className="text-blue-500 shrink-0" size={24} />
          <div>
            <h3 className="text-blue-800 font-[900] text-sm uppercase tracking-widest mb-1">Estimated Time</h3>
            <p className="text-blue-600 font-bold text-xs uppercase tracking-wider">Expected completion in {statusData.estimatedDays} days.</p>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
         {/* Tracking Timeline */}
         <div className="xl:col-span-2 glass-panel p-10 lg:p-14 relative overflow-hidden">
            <div className="absolute top-0 right-0 w-64 h-64 bg-blue-500/5 blur-[100px] rounded-full pointer-events-none" />
            
            <div className="space-y-12 relative">
               {/* Vertical Tracking Line */}
               <div className="absolute left-6 top-0 bottom-0 w-[2px] bg-slate-100 z-0">
                  <motion.div 
                    initial={{ height: 0 }}
                    animate={{ height: '65%' }}
                    transition={{ duration: 2, ease: "easeInOut" }}
                    className="w-full bg-blue-600 rounded-full"
                  />
               </div>

               {statusData?.timeline?.map((step, i) => {
                  const Icon = getStepIcon(step.status);
                  const isLast = i === statusData.timeline.length - 1;
                  const stepStatus = step.status === 'REJECTED' ? 'rejected' : isLast && statusData.status === 'PENDING' ? 'processing' : 'completed';

                  return (
                  <motion.div 
                    key={i}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.1 }}
                    className="flex items-start gap-8 relative z-10"
                  >
                     <div className={`w-12 h-12 rounded-[20px] flex items-center justify-center shrink-0 border-4 border-white shadow-xl transition-all duration-500
                        ${stepStatus === 'completed' ? 'bg-blue-600 text-white' : 
                          stepStatus === 'processing' ? 'bg-amber-100 text-amber-600 animate-pulse border-amber-50' : 
                          stepStatus === 'rejected' ? 'bg-red-100 text-red-600 border-red-50' :
                          'bg-white text-slate-300 border-slate-50'}
                     `}>
                        <Icon size={20} />
                     </div>
                     
                     <div className="flex-1 pt-1">
                        <div className="flex items-center justify-between">
                           <h3 className={`text-lg font-[900] tracking-tight ${stepStatus === 'pending' ? 'text-slate-300' : 'text-slate-900'}`}>
                              {step.status}
                           </h3>
                           <span className={`text-[10px] font-black uppercase tracking-widest ${stepStatus === 'completed' ? 'text-blue-600' : 'text-slate-400'}`}>
                              {new Date(step.timestamp).toLocaleDateString()}
                           </span>
                        </div>
                        <p className={`text-xs font-bold mt-1 uppercase tracking-wider ${stepStatus === 'completed' ? 'text-slate-400' : 'text-slate-500'}`}>
                           {step.note}
                        </p>
                     </div>
                  </motion.div>
                )})}
            </div>
         </div>

         {/* Delivery Summary */}
         <div className="xl:col-span-1 space-y-8">
            <div className="glass-panel p-10 bg-gradient-to-br from-slate-900 to-slate-800 text-white overflow-hidden relative">
               <div className="absolute top-[-20%] right-[-20%] w-40 h-40 bg-blue-500/20 blur-3xl rounded-full" />
               <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-6">Delivery Address</p>
               <div className="flex items-start gap-4 mb-10">
                  <div className="w-10 h-10 rounded-xl bg-white/10 flex items-center justify-center shrink-0">
                     <MapPin size={18} />
                  </div>
                  <div>
                    <p className="text-sm font-bold leading-relaxed uppercase tracking-wider">
                       Daksh Sharma <br />
                       Central Logistics Hub, <br />
                       Sector 5, MIDC, Mumbai <br />
                       Maharashtra - 400093
                    </p>
                  </div>
               </div>
               <div className="flex items-center justify-between pt-8 border-t border-white/10">
                  <div>
                    <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">Carrier</p>
                    <p className="text-xs font-[900] uppercase tracking-widest italic">VIA Priority Express</p>
                  </div>
                  <div className="w-12 h-12 bg-white/10 rounded-2xl flex items-center justify-center">
                    <Truck size={22} />
                  </div>
               </div>
            </div>

            <div className="glass-panel p-8">
               <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-6 italic">Support Line</h3>
               <div className="space-y-4">
                  <p className="text-xs font-bold text-slate-500 leading-relaxed uppercase tracking-wider">For urgent inquiries regarding your delivery, please contact our logistics desk.</p>
                  <button className="w-full py-4 rounded-2xl bg-slate-50 text-[10px] font-black text-slate-900 hover:bg-slate-100 transition-all uppercase tracking-widest flex items-center justify-center gap-2">
                     Support Desk <ChevronRight size={14} />
                  </button>
               </div>
            </div>
         </div>
      </div>
    </div>
  );
}
