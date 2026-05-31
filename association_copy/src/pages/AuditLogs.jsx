import React from 'react';
import { motion } from 'framer-motion';
import { 
  History, 
  Search, 
  Filter, 
  ShieldCheck, 
  Zap, 
  Download, 
  ArrowUpRight, 
  Terminal,
  ChevronRight,
  MoreVertical,
  Activity
} from 'lucide-react';
import { Button } from '../components/ui/Button';

import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getAuditLogs, exportAuditLogs } from '../services/auditService';

export default function AuditLogs() {
  const { execute: fetchLogs, data: logsData, loading: logsLoading } = useApi(getAuditLogs);
  const { execute: doExport } = useApi(exportAuditLogs);
  const [page, setPage] = React.useState(1);

  React.useEffect(() => {
    fetchLogs({}, page, 10);
  }, [fetchLogs, page]);

  const handleExport = async () => {
    const tid = toast.loading('Archiving Ledger...');
    const res = await doExport({});
    if (!res.error) {
      toast.success('Archive Downloaded', { id: tid });
    } else {
      toast.error('Failed to export', { id: tid });
    }
  };

  const logs = logsData?.data?.data || [];
  const totalLogs = logsData?.data?.total || 0;
  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">AUDIT LEDGER</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional event sequence & verification</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2 active:scale-95" onClick={() => toast.success('Live tail connected securely', {icon: '📡'})}>
            <Terminal size={14} />
            Live Tail
          </Button>
          <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20 active:scale-95" onClick={handleExport}>
            <Download size={14} />
            Export Archive
          </Button>
        </div>
      </div>

      {/* Intelligence Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
         {[
           { label: 'Event Velocity', value: '42K/day', drift: 'High Traffic', icon: Activity, color: 'text-blue-500' },
           { label: 'System Integrity', value: '100% Pass', drift: 'Continuous', icon: ShieldCheck, color: 'text-green-500' },
           { label: 'Alert Frequency', value: '2 High/hr', drift: 'Monitoring', icon: Zap, color: 'text-amber-500' },
         ].map((stat, i) => (
           <div key={i} className="glass-panel p-6 flex items-center gap-6">
              <div className={`w-12 h-12 rounded-[20px] bg-slate-50 ${stat.color} flex items-center justify-center shadow-inner`}>
                 <stat.icon size={22} />
              </div>
              <div>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">{stat.label}</p>
                 <div className="flex items-baseline gap-2">
                    <span className="text-xl font-[900] text-slate-900 tracking-tighter italic uppercase">{stat.value}</span>
                    <span className="text-[9px] font-black text-slate-300 uppercase tracking-widest">{stat.drift}</span>
                 </div>
              </div>
           </div>
         ))}
      </div>

      {/* Ledger Panel */}
      <div className="glass-panel overflow-hidden border-slate-100">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
           <div className="relative max-w-sm w-full">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
              <input 
                type="text" 
                placeholder="Search event ID, user or action..." 
                className="w-full pl-12 pr-6 py-4 bg-white border border-slate-100 rounded-[24px] text-xs font-bold outline-none focus:border-blue-500 transition-all shadow-sm"
              />
           </div>
           <div className="flex items-center gap-3">
              <button onClick={() => toast('Severity filter applied')} className="flex items-center gap-2 px-6 py-4 bg-white border border-slate-100 rounded-[24px] text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-all shadow-sm active:scale-95">
                 <Filter size={16} />
                 Severity Filter
              </button>
           </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-slate-50/20">
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Event Identity / Epoch</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Institutional Actor</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Operation Mapping</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">System Component</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Intel</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {logsLoading ? (
                <tr><td colSpan={5} className="p-8 text-center text-xs font-bold text-slate-400">Loading audit ledger...</td></tr>
              ) : logs.map((log, i) => {
                const isHighAlert = log.action.includes('DELETE') || log.action.includes('UPDATE_ROLE');
                const isLowAlert = log.action.includes('VIEW') || log.action.includes('SYNC');
                return (
                <motion.tr 
                  key={log.id}
                  initial={{ opacity: 0, y: 5 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.05 }}
                  className="group hover:bg-slate-50/50 transition-all cursor-pointer"
                  onClick={() => toast(`Details for ${log.id}:\n${JSON.stringify(log.changes || {}, null, 2)}`, { duration: 4000 })}
                >
                  <td className="px-8 py-6">
                    <div className="flex flex-col">
                       <span className="text-xs font-[900] text-slate-900 tracking-tight italic group-hover:text-blue-600 transition-colors uppercase">#{log.id}</span>
                       <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{log.timestamp}</span>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-3">
                       <div className="w-10 h-10 rounded-xl bg-white border border-slate-100 flex items-center justify-center text-slate-400 group-hover:text-blue-600 transition-colors shadow-sm">
                          <History size={16} />
                       </div>
                       <span className="text-xs font-black text-slate-800 tracking-tight uppercase">{log.staffName}</span>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <span className={`text-[9px] font-black uppercase tracking-widest px-3 py-1.5 rounded-lg
                       ${isHighAlert ? 'bg-red-50 text-red-600' : isLowAlert ? 'bg-green-50 text-green-600' : 'bg-blue-50 text-blue-600'}
                    `}>{log.action}</span>
                  </td>
                  <td className="px-8 py-6">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{log.target}</span>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center justify-center gap-2">
                       <button onClick={(e) => {e.stopPropagation(); toast(`Details:\n${JSON.stringify(log.changes || {}, null, 2)}`);}} className="p-3 bg-white border border-slate-100 rounded-xl text-slate-200 hover:text-blue-600 transition-all shadow-sm active:scale-95">
                         <ArrowUpRight size={14} />
                       </button>
                    </div>
                  </td>
                </motion.tr>
              )})}
            </tbody>
          </table>
        </div>
        
        <div className="p-10 bg-slate-50/30 border-t border-slate-100 flex items-center justify-between">
           <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Institutional Event Archive • {totalLogs} Records Secured</p>
           <div className="flex items-center gap-4">
              <button disabled={page === 1} onClick={() => setPage(p => p - 1)} className="px-6 py-3 border border-slate-200 rounded-[14px] text-[10px] font-black text-slate-400 bg-white hover:bg-slate-50 transition-all uppercase tracking-widest active:scale-95 disabled:opacity-50">Archive Previous</button>
              <button onClick={() => setPage(p => p + 1)} className="px-6 py-3 border border-slate-200 rounded-[14px] text-[10px] font-black text-slate-900 bg-white hover:bg-slate-50 transition-all uppercase tracking-widest active:scale-95">History Forward</button>
           </div>
        </div>
      </div>
    </div>
  );
}
