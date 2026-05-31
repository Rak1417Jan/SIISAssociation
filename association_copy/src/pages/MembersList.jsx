import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Search, 
  Filter, 
  Download, 
  Plus, 
  MoreHorizontal, 
  User, 
  Mail, 
  Phone,
  Building2,
  ChevronRight,
  ArrowUpRight,
  ShieldCheck,
  Zap
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMembers, searchMembers, exportMembers } from '../services/memberService';

export default function MembersList() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState({});
  const [membersList, setMembersList] = useState([]);
  const [totalPages, setTotalPages] = useState(1);
  
  const { execute: fetchMembers, loading: loading1 } = useApi(getMembers);
  const { execute: fetchSearch, loading: loading2 } = useApi(searchMembers);
  const { execute: doExport } = useApi(exportMembers);
  
  const loading = loading1 || loading2;

  useEffect(() => {
    const timer = setTimeout(async () => {
      if (search) {
        const res = await fetchSearch(search);
        if (res.data) {
          setMembersList(res.data);
          setTotalPages(1);
        }
      } else {
        const res = await fetchMembers(filters, page, 10);
        if (res.data) {
          setMembersList(res.data.data);
          setTotalPages(Math.ceil(res.data.total / 10) || 1);
        }
      }
    }, 300);
    return () => clearTimeout(timer);
    // eslint-disable-next-line
  }, [search, filters, page]);

  const handleExport = async () => {
    const tid = toast.loading('Exporting Database...');
    const res = await doExport(filters);
    if (!res.error) toast.success(`Exported ${res.data.fileName}`, {id: tid});
    else toast.error('Export failed', {id: tid});
  };

  return (
    <div className="space-y-10">
      {/* Header Segment */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">MEMBER DIRECTORY</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional database management</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2 active:scale-95" onClick={handleExport}>
            <Download size={14} />
            Export DB
          </Button>
          <Button size="sm" onClick={() => navigate('/register')} className="gap-2 shadow-xl shadow-blue-500/20 active:scale-95">
            <Plus size={16} />
            Add Entity
          </Button>
        </div>
      </div>

      {/* Intelligence & Stats Bar */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
         {[
           { label: 'Active Personnel', value: '4,028', drift: 'Normal', icon: User, color: 'text-blue-500' },
           { label: 'Verification Velocity', value: '142/mo', drift: '+12%', icon: ShieldCheck, color: 'text-green-500' },
           { label: 'Elite Density', value: '18.4%', drift: '+2.1%', icon: Zap, color: 'text-amber-500' },
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

      {/* Data Table Panel */}
      <div className="glass-panel overflow-hidden border-slate-100">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
           <div className="relative max-w-sm w-full">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
              <input 
                type="text" 
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search by name, ID or phone..." 
                className="w-full pl-12 pr-6 py-4 bg-white border border-slate-100 rounded-[20px] text-xs font-bold outline-none focus:border-blue-500 transition-all shadow-sm"
              />
           </div>
           <div className="flex items-center gap-3">
              <button onClick={() => toast('Filters applied', {icon:'🎛️'})} className="flex items-center gap-2 px-6 py-4 bg-white border border-slate-100 rounded-[20px] text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-all shadow-sm active:scale-95">
                 <Filter size={16} />
                 Filter Results
              </button>
           </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-slate-50/20">
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Entity ID / Identity</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Institutional Background</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Enrollment Date</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Status Index</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Intelligence</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {loading ? (
                 <tr>
                   <td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">Loading data...</td>
                 </tr>
              ) : membersList.length === 0 ? (
                 <tr>
                   <td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">No members found</td>
                 </tr>
              ) : (
                membersList.map((member, i) => (
                <motion.tr 
                  key={member.id}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: i * 0.05 }}
                  onClick={() => navigate(`/member/${member.id}`)}
                  className="group hover:bg-slate-50/50 transition-all cursor-pointer"
                >
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-4">
                       <div className="relative">
                          <div className="w-12 h-12 rounded-2xl bg-white border border-slate-100 flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors shadow-sm overflow-hidden">
                             <User size={20} />
                          </div>
                          <div className={`absolute -bottom-1 -right-1 w-4 h-4 rounded-full border-4 border-white ${member.status === 'APPROVED' ? 'bg-green-500' : member.status === 'REJECTED' ? 'bg-red-500' : 'bg-amber-400'}`} />
                       </div>
                       <div className="flex flex-col">
                          <span className="text-sm font-[900] text-slate-900 tracking-tight italic group-hover:text-blue-600 transition-colors uppercase">{member.name}</span>
                          <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{member.id}</span>
                       </div>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex flex-col">
                       <div className="flex items-center gap-2 mb-1">
                          <Building2 size={12} className="text-slate-300" />
                          <span className="text-xs font-black text-slate-800 tracking-tight">{member.firmName}</span>
                       </div>
                       <div className="flex items-center gap-2">
                          <Mail size={12} className="text-slate-300" />
                          <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{member.email}</span>
                       </div>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <span className="text-[10px] font-black text-slate-900 uppercase tracking-widest px-3 py-1.5 bg-slate-50 rounded-lg">{member.appliedAt}</span>
                  </td>
                  <td className="px-8 py-6 text-center">
                    <Badge status={member.status.toLowerCase()} label={member.status} />
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center justify-center gap-2">
                       <button onClick={(e) => {e.stopPropagation(); toast(`Opening quick view for ${member.name}`);}} className="p-3 bg-white border border-slate-100 rounded-xl text-slate-300 hover:text-blue-600 transition-all shadow-sm active:scale-95">
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
           <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Intel Ledger Page {page} of {totalPages}</p>
           <div className="flex items-center gap-4">
              <button onClick={() => setPage(p => Math.max(1, p - 1))} className="w-10 h-10 border border-slate-200 rounded-xl flex items-center justify-center text-slate-400 bg-white hover:bg-slate-50 transition-all active:scale-95">
                <ChevronRight className="rotate-180" size={16} />
              </button>
              <div className="flex items-center gap-2">
                 {Array.from({length: totalPages}).map((_, idx) => {
                   const n = idx + 1;
                   return (
                   <button onClick={() => setPage(n)} key={n} className={`w-10 h-10 rounded-xl text-[10px] font-black transition-all ${n === page ? 'bg-blue-600 text-white shadow-lg shadow-blue-500/20' : 'bg-white text-slate-400 hover:text-slate-900'}`}>
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
    </div>
  );
}
