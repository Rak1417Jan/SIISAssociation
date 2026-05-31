import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Search, 
  MapPin, 
  Building2, 
  User, 
  Globe, 
  MessageCircle, 
  ExternalLink,
  Filter,
  ArrowRight,
  ShieldCheck,
  Zap,
  MoreVertical,
  Phone,
  Mail
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getDirectory, getIndustryList, sendConnectionRequest } from '../services/engagementService';
import { usePermissions } from '../hooks/usePermissions';
import PermissionGate from '../components/PermissionGate';

export default function MemberDirectory() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [industryFilter, setIndustryFilter] = useState('All');
  const [planFilter, setPlanFilter] = useState('All');
  
  const { hasPermission } = usePermissions();
  const canViewContact = hasPermission('viewContact', 'directory');

  const { execute: fetchDirectory, data: directoryData, loading } = useApi(getDirectory);
  const { execute: fetchIndustries, data: industriesData } = useApi(getIndustryList);
  const { execute: doConnect } = useApi(sendConnectionRequest);

  useEffect(() => {
    fetchIndustries();
  }, [fetchIndustries]);

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchDirectory({ search, industry: industryFilter, planType: planFilter }, page, 12);
    }, 300);
    return () => clearTimeout(timer);
  }, [search, industryFilter, planFilter, page, fetchDirectory]);

  const handleContact = async (memberId, name) => {
    const tid = toast.loading(`Initiating contact with ${name}...`);
    const res = await doConnect('CURRENT_USER', memberId, 'Hi, I would like to connect!');
    if (!res.error) toast.success(res.data.message, { id: tid });
    else toast.error('Failed to connect', { id: tid });
  };

  const handleExternal = (name) => {
    toast(`Fetching external institutional dossier for ${name}`, { icon: '🔍' });
  };

  const handleExport = () => {
    const tId = toast.loading('Exporting Global Handshake index...');
    setTimeout(() => {
      toast.success('Global Handshake index exported', { id: tId });
    }, 1500);
  };

  const members = directoryData?.data?.data || [];
  const total = directoryData?.data?.total || 0;
  const industries = industriesData?.data || ['All'];

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">ASSOCIATION NETWORK</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Connect with verified institutional partners</p>
        </div>
        <div className="flex items-center gap-3">
          <Button size="sm" className="gap-2" onClick={() => toast.success('Network topographical view initialized')}>
            <Globe size={14} />
            Network View
          </Button>
        </div>
      </div>

      {/* Global Intelligence Bar */}
      <div className="glass-panel p-8 bg-slate-900 overflow-hidden relative">
         <div className="absolute top-0 right-0 w-64 h-64 bg-blue-600/10 blur-[100px] rounded-full pointer-events-none" />
         <div className="flex flex-col md:flex-row items-center gap-6 relative z-10">
            <div className="flex-1 w-full relative">
               <Search className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-500" size={20} />
               <input 
                  type="text" 
                  value={search}
                  onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                  placeholder="Search network entities by name, region or industry..." 
                  className="w-full pl-16 pr-6 py-5 bg-white/5 border border-white/10 rounded-2xl text-white font-bold outline-none focus:bg-white focus:text-slate-900 transition-all shadow-xl"
               />
            </div>
            <div className="flex items-center gap-4">
              <select 
                value={industryFilter}
                onChange={(e) => { setIndustryFilter(e.target.value); setPage(1); }}
                className="bg-white/10 border border-white/20 text-white text-xs font-bold py-4 px-6 rounded-xl outline-none cursor-pointer"
              >
                {industries.map(ind => <option key={ind} value={ind} className="text-slate-900">{ind}</option>)}
              </select>
              <select 
                value={planFilter}
                onChange={(e) => { setPlanFilter(e.target.value); setPage(1); }}
                className="bg-white/10 border border-white/20 text-white text-xs font-bold py-4 px-6 rounded-xl outline-none cursor-pointer"
              >
                <option value="All" className="text-slate-900">All Tiers</option>
                <option value="YEARLY" className="text-slate-900">Yearly</option>
                <option value="LIFETIME" className="text-slate-900">Lifetime</option>
              </select>
            </div>
            <div className="flex items-center gap-8 px-4 hidden lg:flex">
               <div>
                  <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Total Nodes</p>
                  <p className="text-xl font-[900] text-white tracking-tighter italic">{total}+</p>
               </div>
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
         {loading ? (
           <div className="col-span-full py-20 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">Loading network nodes...</div>
         ) : members.length === 0 ? (
           <div className="col-span-full py-20 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">No members found matching your search.</div>
         ) : members.map((m, i) => (
           <motion.div 
             key={m.id}
             initial={{ opacity: 0, y: 20 }}
             animate={{ opacity: 1, y: 0 }}
             transition={{ delay: i * 0.05 }}
             className="glass-panel p-8 group hover:shadow-[0_40px_80px_-20px_rgba(59,130,246,0.12)] transition-all duration-500 border-slate-100 text-center flex flex-col justify-between"
           >
              <div>
                <div className="relative inline-block mb-6">
                   <div className="w-20 h-20 rounded-[28px] bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-300 overflow-hidden shadow-inner">
                      <img src={m.photo} alt={m.name} className="w-full h-full object-cover" />
                   </div>
                   <div className={`absolute -bottom-1 -right-1 w-6 h-6 rounded-full border-4 border-white flex items-center justify-center
                     ${m.planType === 'LIFETIME' ? 'bg-amber-400' : 'bg-blue-500'}
                   `}>
                      <Zap size={10} className="text-white" />
                   </div>
                </div>

                <div className="flex items-center justify-center gap-2 mb-1">
                  <h3 className="text-lg font-[900] text-slate-900 tracking-tight uppercase italic">{m.name}</h3>
                  {m.isVerified && <ShieldCheck size={14} className="text-blue-500" title="Verified Member" />}
                </div>
                <p className={`text-[9px] font-black uppercase tracking-widest mb-6 ${m.planType === 'LIFETIME' ? 'text-amber-500' : 'text-slate-400'}`}>
                  {m.planType} Partner • {m.industry}
                </p>

                <div className="space-y-4 pt-6 border-t border-slate-50 text-left">
                   <div className="flex items-center gap-3">
                      <Building2 size={14} className="text-slate-300" />
                      <span className="text-[10px] font-black text-slate-700 uppercase tracking-widest truncate">{m.firmName}</span>
                   </div>
                   <div className="flex items-center gap-3">
                      <MapPin size={14} className="text-slate-300" />
                      <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest truncate">{m.city}</span>
                   </div>
                   
                   {canViewContact && (
                     <>
                       <div className="flex items-center gap-3">
                          <Phone size={14} className="text-slate-300" />
                          <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest truncate">{m.phone}</span>
                       </div>
                       <div className="flex items-center gap-3">
                          <Mail size={14} className="text-slate-300" />
                          <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest truncate">{m.email}</span>
                       </div>
                     </>
                   )}
                </div>
              </div>

              <div className="mt-8 flex items-center gap-2">
                 <button 
                   onClick={() => handleContact(m.id, m.name)}
                   className="flex-1 py-3 bg-slate-900 text-white rounded-xl text-[9px] font-black uppercase tracking-widest hover:bg-blue-600 transition-colors shadow-lg shadow-slate-900/10 active:scale-[0.98]"
                 >
                   Establish Contact
                 </button>
                 <button 
                   onClick={() => handleExternal(m.name)}
                   className="p-3 bg-slate-50 border border-slate-100 rounded-xl text-slate-400 hover:text-slate-900 transition-all active:scale-[0.98]"
                 >
                   <ExternalLink size={14} />
                 </button>
              </div>
           </motion.div>
         ))}
      </div>
      
      {totalPages > 1 && (
        <div className="flex justify-center gap-4 py-4">
          <button onClick={() => setPage(p => Math.max(1, p-1))} className="px-6 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-500 shadow-sm active:scale-95">Prev</button>
          <button onClick={() => setPage(p => Math.min(Math.ceil(total/12), p+1))} className="px-6 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-900 shadow-sm active:scale-95">Next</button>
        </div>
      )}

      {/* Intelligence Banner */}
      <div className="glass-panel p-10 bg-blue-50/50 border-blue-100 flex flex-col sm:flex-row sm:items-center justify-between gap-6">
         <div className="flex items-center gap-6">
            <div className="w-14 h-14 rounded-2xl bg-white border border-blue-100 flex items-center justify-center text-blue-600 shadow-sm shrink-0">
               <ShieldCheck size={28} />
            </div>
            <div>
               <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic">Verified Data Only</h3>
               <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mt-1">All network participants have clear institutional verification</p>
            </div>
         </div>
         <Button onClick={handleExport} className="font-black text-[10px] uppercase tracking-widest px-10 whitespace-nowrap">
            Export Global Handshake
         </Button>
      </div>
    </div>
  );
}
