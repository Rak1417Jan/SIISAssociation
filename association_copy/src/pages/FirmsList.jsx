import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Building2, 
  Search, 
  Filter, 
  Download, 
  Plus, 
  MapPin, 
  Users, 
  TrendingUp, 
  ArrowUpRight,
  MoreVertical,
  ChevronRight,
  ShieldCheck,
  Globe
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getFirms, searchFirms, exportFirms, createFirm } from '../services/firmService';

export default function FirmsList() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState({});
  const [firmsList, setFirmsList] = useState([]);
  const [totalPages, setTotalPages] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newFirmName, setNewFirmName] = useState('');
  
  const { execute: fetchFirms, loading: loading1 } = useApi(getFirms);
  const { execute: fetchSearch, loading: loading2 } = useApi(searchFirms);
  const { execute: doExport } = useApi(exportFirms);
  const { execute: doCreate } = useApi(createFirm);
  
  const loading = loading1 || loading2;

  useEffect(() => {
    const timer = setTimeout(async () => {
      if (search) {
        const res = await fetchSearch(search);
        if (res.data) {
          setFirmsList(res.data);
          setTotalPages(1);
        }
      } else {
        const res = await fetchFirms(filters, page, 10);
        if (res.data) {
          setFirmsList(res.data.data);
          setTotalPages(Math.ceil(res.data.total / 10) || 1);
        }
      }
    }, 300);
    return () => clearTimeout(timer);
    // eslint-disable-next-line
  }, [search, filters, page]);

  const handleCreate = async () => {
    if (!newFirmName) return;
    const tid = toast.loading('Registering firm...');
    const res = await doCreate({ name: newFirmName });
    if (!res.error) {
       toast.success('Firm registered!', {id: tid});
       setIsModalOpen(false);
       setNewFirmName('');
       navigate(`/firm-editor?id=${res.data.firm.id}`);
    } else {
       toast.error('Failed to register', {id: tid});
    }
  };

  const handleExport = async () => {
    const tid = toast.loading('Exporting Analytics...');
    const res = await doExport(filters);
    if (!res.error) toast.success(`Exported ${res.data.fileName}`, {id: tid});
  };

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">INSTITUTIONAL INDEX</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Manage affiliated corporations & firms</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2 active:scale-95" onClick={handleExport}>
            <Download size={14} />
            Analytics Export
          </Button>
          <Button size="sm" onClick={() => setIsModalOpen(true)} className="gap-2 shadow-xl shadow-indigo-500/20 bg-indigo-600 hover:bg-indigo-700 active:scale-95">
            <Plus size={16} />
            Register Firm
          </Button>
        </div>
      </div>

      {/* Corporate IQ Bar */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
         {[
           { label: 'Total Affiliates', value: '892', drift: 'Steady', icon: Building2, color: 'text-indigo-500' },
           { label: 'Market Capital', value: '₹142M', drift: '+14%', icon: TrendingUp, color: 'text-green-500' },
           { label: 'Global Footprint', value: '24 Regions', drift: 'Expanding', icon: Globe, color: 'text-blue-500' },
         ].map((stat, i) => (
           <div key={i} className="glass-panel p-6 flex items-center gap-6">
              <div className={`w-12 h-12 rounded-[20px] bg-slate-50 ${stat.color} flex items-center justify-center shadow-inner`}>
                 <stat.icon size={22} />
              </div>
              <div>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">{stat.label}</p>
                 <div className="flex items-baseline gap-2">
                    <span className="text-xl font-[900] text-slate-900 tracking-tighter italic">{stat.value}</span>
                    <span className="text-[9px] font-black text-slate-300 uppercase tracking-widest">{stat.drift}</span>
                 </div>
              </div>
           </div>
         ))}
      </div>

      {/* Index Panel */}
      <div className="glass-panel overflow-hidden border-slate-100">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
           <div className="relative max-w-sm w-full">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
              <input 
                type="text" 
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search institutional ID or name..." 
                className="w-full pl-12 pr-6 py-4 bg-white border border-slate-100 rounded-[24px] text-xs font-bold outline-none focus:border-indigo-500 transition-all shadow-sm"
              />
           </div>
           <div className="flex items-center gap-3">
              <button onClick={() => toast('Re-indexing array by Institutional IQ...', {icon: '🔄'})} className="flex items-center gap-2 px-6 py-4 bg-white border border-slate-100 rounded-[24px] text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-all shadow-sm active:scale-95">
                 <Filter size={16} />
                 Sort by IQ
              </button>
           </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-slate-50/20">
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Institutional Identity</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Headquarters</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-right">Human Capital</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-right">Tax Revenue</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Trust Status</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Intel</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {loading ? (
                 <tr>
                   <td colSpan="6" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">Loading data...</td>
                 </tr>
              ) : firmsList.length === 0 ? (
                 <tr>
                   <td colSpan="6" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">No firms found</td>
                 </tr>
              ) : (
                firmsList.map((firm, i) => (
                <motion.tr 
                  key={firm.id}
                  initial={{ opacity: 0, scale: 0.98 }}
                  animate={{ opacity: 1, scale: 1 }}
                  transition={{ delay: i * 0.05 }}
                  className="group hover:bg-slate-50/50 transition-all cursor-pointer"
                  onClick={() => navigate(`/firm-editor?id=${firm.id}`)}
                >
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-4">
                       <div className="w-12 h-12 rounded-2xl bg-white border border-slate-100 flex items-center justify-center text-slate-300 group-hover:text-indigo-600 transition-colors shadow-sm">
                          <Building2 size={20} />
                       </div>
                       <div className="flex flex-col">
                          <span className="text-sm font-[900] text-slate-900 tracking-tight italic group-hover:text-indigo-600 transition-colors uppercase">{firm.name}</span>
                          <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{firm.id}</span>
                       </div>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-2">
                       <MapPin size={12} className="text-slate-300" />
                       <span className="text-[10px] font-black text-slate-700 uppercase tracking-widest">{firm.registrationNo}</span>
                    </div>
                  </td>
                  <td className="px-8 py-6 text-right">
                    <div className="flex items-center justify-end gap-2 text-slate-900">
                       <div className="flex flex-col text-right">
                          <span className="text-xs font-[900] italic tracking-tighter">{firm.memberCount} ENTITIES</span>
                          <span className="text-[8px] font-black text-slate-300 uppercase tracking-widest leading-none">Affiliated</span>
                       </div>
                    </div>
                  </td>
                  <td className="px-8 py-6 text-right">
                    <span className="text-sm font-[900] text-indigo-600 font-mono italic tracking-tighter">{firm.industry}</span>
                  </td>
                  <td className="px-8 py-6 text-center">
                    <Badge status={firm.status.toLowerCase()} label={firm.status} />
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center justify-center gap-2">
                       <button onClick={(e) => {e.stopPropagation(); navigate(`/firm-editor?id=${firm.id}`);}} className="p-3 bg-white border border-slate-100 rounded-xl text-slate-300 hover:text-indigo-600 transition-all shadow-sm active:scale-95">
                         <ArrowUpRight size={16} />
                       </button>
                    </div>
                  </td>
                </motion.tr>
              )))}
            </tbody>
          </table>
        </div>
        
        <div className="p-10 bg-slate-50/30 border-t border-slate-100 flex items-center justify-between">
           <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Institutional Index Ledger • Page {page} of {totalPages}</p>
           <div className="flex items-center gap-4">
              <button onClick={() => setPage(p => Math.max(1, p - 1))} className="w-10 h-10 border border-slate-200 rounded-xl flex items-center justify-center text-slate-400 bg-white hover:bg-slate-50 transition-all active:scale-95">
                <ChevronRight className="rotate-180" size={16} />
              </button>
              <div className="flex items-center gap-2">
                 {Array.from({length: totalPages}).map((_, idx) => {
                   const n = idx + 1;
                   return (
                   <button onClick={() => setPage(n)} key={n} className={`w-10 h-10 rounded-xl text-[10px] font-black transition-all ${n === page ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/20' : 'bg-white text-slate-400 hover:text-slate-900'}`}>
                     {n}
                   </button>
                 )})}
              </div>
              <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} className="w-10 h-10 border border-slate-200 rounded-xl flex items-center justify-center text-slate-400 bg-white hover:bg-slate-50 transition-all active:scale-95">
                <ChevronRight size={16} />
              </button>
           </div>
        </div>
      </div>

      {isModalOpen && (
         <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={() => setIsModalOpen(false)} />
            <motion.div 
               initial={{ opacity: 0, scale: 0.95, y: 20 }}
               animate={{ opacity: 1, scale: 1, y: 0 }}
               className="relative bg-white rounded-[40px] w-full max-w-lg p-10 shadow-2xl"
            >
               <h2 className="text-2xl font-[900] text-slate-900 uppercase italic mb-6">Register New Firm</h2>
               <input 
                  type="text" 
                  value={newFirmName}
                  onChange={(e) => setNewFirmName(e.target.value)}
                  placeholder="Enter Firm Name..." 
                  className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-[20px] text-sm font-black outline-none focus:border-indigo-500 focus:bg-white transition-all shadow-inner mb-6"
               />
               <div className="flex justify-end gap-4">
                 <button onClick={() => setIsModalOpen(false)} className="text-[10px] font-black uppercase text-slate-400 hover:text-slate-900">Cancel</button>
                 <Button onClick={handleCreate} className="bg-indigo-600 hover:bg-indigo-700 py-4 px-8 text-xs font-black uppercase rounded-2xl">Create Profile</Button>
               </div>
            </motion.div>
         </div>
      )}
    </div>
  );
}
