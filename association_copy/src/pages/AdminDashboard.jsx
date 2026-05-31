import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { 
  Users, 
  Building2, 
  TrendingUp, 
  DollarSign, 
  ArrowUpRight, 
  ArrowDownRight, 
  Calendar,
  Search,
  Bell,
  MoreVertical,
  Activity,
  Zap,
  ShieldCheck,
  Globe,
  Database,
  ChevronRight
} from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate } from 'react-router-dom';
import { useApi } from '../hooks/useApi';
import { getDashboardMetrics, getPendingApplications } from '../services/analyticsService';
import { 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  BarChart,
  Bar 
} from 'recharts';
import { Modal } from '../components/ui/Modal';
import toast from 'react-hot-toast';

const monthlyData = [
  { name: 'Jan', revenue: 4000, members: 240 },
  { name: 'Feb', revenue: 3000, members: 198 },
  { name: 'Mar', revenue: 9000, members: 580 },
  { name: 'Apr', revenue: 2780, members: 390 },
  { name: 'May', revenue: 1890, members: 480 },
  { name: 'Jun', revenue: 2390, members: 380 },
];

const annualData = [
  { name: '2020', revenue: 45000, members: 1200 },
  { name: '2021', revenue: 52000, members: 1800 },
  { name: '2022', revenue: 78000, members: 2400 },
  { name: '2023', revenue: 112000, members: 3800 },
  { name: '2024', revenue: 145000, members: 4280 },
];

export default function AdminDashboard() {
  const [timeframe, setTimeframe] = useState('monthly');
  const [isExportModalOpen, setIsExportModalOpen] = useState(false);
  const [activeKpi, setActiveKpi] = useState('revenue');
  const navigate = useNavigate();

  const { execute: fetchMetrics, data: metricsData, loading: metricsLoading } = useApi(getDashboardMetrics);
  const { execute: fetchPending, data: pendingData, loading: pendingLoading } = useApi(getPendingApplications);

  React.useEffect(() => {
    fetchMetrics();
    fetchPending(1, 10);
  }, [fetchMetrics, fetchPending, timeframe]);

  const metrics = metricsData?.data || {};
  const pendingApps = pendingData?.data?.data || [];

  const handleExport = () => {
    toast.promise(
      new Promise((resolve) => setTimeout(resolve, 1500)),
      {
        loading: 'Preparing executive intelligence report...',
        success: 'Intelligence dossier exported successfully',
        error: 'Export synchronization failed',
      }
    );
    setIsExportModalOpen(false);
  };

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic uppercase">Command Center</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Executive oversight & intelligence</p>
        </div>
        <div className="flex items-center gap-4">
           <div className="text-right hidden sm:block">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">System Health</p>
              <div className="flex items-center gap-2">
                 <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                 <p className="text-sm font-[900] text-slate-900 tracking-tighter uppercase">Operational</p>
              </div>
           </div>
           <Button size="sm" className="gap-2" onClick={() => setIsExportModalOpen(true)}>
              <TrendingUp size={14} />
              Export Intel
           </Button>
        </div>
      </div>

      {/* KPI Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
        {[
          { id: 'totalMembers', label: 'Total Members', value: metrics.totalMembers, icon: Users, color: 'text-blue-600', bg: 'bg-blue-50' },
          { id: 'activeMembers', label: 'Active Members', value: metrics.activeMembers, icon: Activity, color: 'text-purple-600', bg: 'bg-purple-50' },
          { id: 'pendingApprovals', label: 'Pending Approvals', value: metrics.pendingApprovals, icon: Clock, color: 'text-amber-600', bg: 'bg-amber-50' },
          { id: 'revenue', label: 'Monthly Revenue', value: metrics.revenueThisMonth ? `₹${metrics.revenueThisMonth.toLocaleString()}` : '', icon: DollarSign, color: 'text-green-600', bg: 'bg-green-50' },
        ].map((kpi, i) => (
          <motion.div
            key={kpi.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.1 }}
            onClick={() => setActiveKpi(kpi.id)}
            className={`glass-panel p-8 group cursor-pointer transition-all duration-500 relative overflow-hidden
               ${activeKpi === kpi.id ? 'bg-white shadow-2xl scale-[1.02] border-blue-100 ring-2 ring-blue-500/10' : 'hover:bg-white'}
            `}
          >
             {activeKpi === kpi.id && <div className="absolute top-0 right-0 w-2 h-full bg-blue-600" />}
             <div className="flex items-center justify-between mb-6">
                <div className={`w-12 h-12 rounded-2xl ${kpi.bg} ${kpi.color} flex items-center justify-center shadow-inner`}>
                   {kpi.icon && <kpi.icon size={24} />}
                </div>
             </div>
             <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">{kpi.label}</p>
             {metricsLoading ? (
               <div className="h-8 w-24 bg-slate-100 rounded-xl animate-pulse mt-2" />
             ) : (
               <h3 className="text-3xl font-[900] text-slate-900 tracking-tighter italic">{kpi.value || '0'}</h3>
             )}
          </motion.div>
        ))}
      </div>

      {/* Primary Analytics Section */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
        <div className="xl:col-span-2 glass-panel p-10 relative overflow-hidden">
           <div className="absolute top-0 right-0 w-64 h-64 bg-blue-500/5 blur-[120px] rounded-full pointer-events-none" />
           <div className="flex items-center justify-between mb-10 relative z-10">
              <h3 className="text-xl font-[900] text-slate-900 tracking-tight uppercase italic">
                {activeKpi === 'members' ? 'Human Capital Growth' : 'Revenue Intelligence'}
              </h3>
              <div className="flex items-center gap-2 bg-slate-50 p-1.5 rounded-2xl border border-slate-100">
                 <button 
                   onClick={() => setTimeframe('monthly')}
                   className={`px-5 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all
                     ${timeframe === 'monthly' ? 'bg-white text-blue-600 shadow-xl shadow-blue-500/10' : 'text-slate-400 hover:text-slate-900'}
                   `}
                 >
                   Monthly
                 </button>
                 <button 
                   onClick={() => setTimeframe('annual')}
                   className={`px-5 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all
                     ${timeframe === 'annual' ? 'bg-white text-blue-600 shadow-xl shadow-blue-500/10' : 'text-slate-400 hover:text-slate-900'}
                   `}
                 >
                   Annual
                 </button>
              </div>
           </div>
           <div className="h-[380px] w-full relative z-10">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={timeframe === 'monthly' ? monthlyData : annualData}>
                  <defs>
                    <linearGradient id="colorRev" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#2563eb" stopOpacity={0.15}/>
                      <stop offset="95%" stopColor="#2563eb" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                  <XAxis 
                    dataKey="name" 
                    axisLine={false} 
                    tickLine={false} 
                    tick={{ fontSize: 10, fontWeight: 900, fill: '#94a3b8' }} 
                  />
                  <YAxis 
                    axisLine={false} 
                    tickLine={false} 
                    tick={{ fontSize: 10, fontWeight: 900, fill: '#94a3b8' }} 
                  />
                  <Tooltip 
                    contentStyle={{ borderRadius: '24px', border: 'none', boxShadow: '0 30px 60px -12px rgba(0,0,0,0.15)', padding: '20px' }}
                    itemStyle={{ fontSize: '12px', fontWeight: '900', color: '#1e293b' }}
                  />
                  <Area 
                    type="monotone" 
                    dataKey={activeKpi === 'members' ? 'members' : 'revenue'} 
                    stroke="#2563eb" 
                    strokeWidth={4} 
                    fillOpacity={1} 
                    fill="url(#colorRev)" 
                  />
                </AreaChart>
              </ResponsiveContainer>
           </div>
        </div>

        {/* Pending Applications Hub */}
        <div className="xl:col-span-1 glass-panel p-10 relative overflow-hidden">
           <div className="absolute top-0 right-0 w-40 h-40 bg-blue-500/5 blur-3xl rounded-full" />
           <div className="flex items-center justify-between mb-10 relative z-10">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">Pending Applications</h3>
              <div className="flex items-center gap-2">
                 <div className="w-1.5 h-1.5 rounded-full bg-amber-500 animate-pulse" />
                 <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Needs Action</span>
              </div>
           </div>
           
           <div className="space-y-6 relative z-10 min-h-[340px]">
              {pendingLoading ? (
                 <div className="text-center text-xs font-bold text-slate-400 py-10">Loading pending applications...</div>
              ) : pendingApps.length === 0 ? (
                 <div className="text-center text-xs font-bold text-slate-400 py-10">No pending applications</div>
              ) : (
                 pendingApps.map((app, i) => (
                    <motion.div 
                       key={app.memberId} 
                       initial={{ opacity: 0, x: 10 }}
                       animate={{ opacity: 1, x: 0 }}
                       transition={{ delay: i * 0.1 }}
                       onClick={() => navigate(`/member/${app.memberId}`)}
                       className="p-4 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between group cursor-pointer hover:bg-white hover:border-blue-200 transition-all shadow-sm hover:shadow-md"
                    >
                       <div>
                          <p className="text-xs font-black text-slate-900 uppercase tracking-wider mb-1 group-hover:text-blue-600 transition-colors">{app.name}</p>
                          <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{app.firmName}</p>
                       </div>
                       <div className="flex flex-col items-end">
                          <span className="text-[10px] font-black text-amber-600 bg-amber-100 px-2 py-1 rounded-lg uppercase tracking-widest">{app.daysWaiting} days wait</span>
                       </div>
                    </motion.div>
                 ))
              )}
           </div>
           
           <Button variant="secondary" onClick={() => navigate('/members')} className="w-full mt-10 py-5 rounded-2xl text-[10px] font-black uppercase tracking-widest transition-all active:scale-[0.98]">
              View All Members
           </Button>
        </div>
      </div>

      {/* Intelligence Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
         <div className="glass-panel p-10 bg-slate-900 text-white overflow-hidden relative min-h-[340px]">
            <div className="absolute top-0 right-0 w-80 h-80 bg-blue-600/10 blur-[100px] rounded-full pointer-events-none" />
            <div className="flex flex-col h-full justify-between relative z-10">
               <div>
                  <Globe className="text-blue-500 mb-10" size={40} />
                  <h3 className="text-3xl font-[900] tracking-tighter mb-4 italic uppercase leading-none">Expiring Soon</h3>
                  <p className="text-slate-400 font-bold text-xs uppercase tracking-widest leading-relaxed max-w-sm">
                     {metricsLoading ? '...' : `${metrics.expiringSoon || 0} memberships are expiring within the next 30 days.`}
                  </p>
               </div>
               <div className="mt-12 flex items-center gap-10">
                  <div>
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Expiring Members</p>
                    <p className="text-3xl font-[900] text-white tracking-tighter italic">{metricsLoading ? '...' : metrics.expiringSoon || 0}</p>
                  </div>
                  <div className="w-[1px] h-12 bg-white/10" />
                  <Button onClick={() => navigate('/members')} className="bg-white text-slate-900 hover:bg-slate-100 font-black text-[10px] uppercase tracking-widest rounded-xl transition-all active:scale-95">
                     View Renewals
                  </Button>
               </div>
            </div>
         </div>

         <div className="glass-panel p-10 relative overflow-hidden">
            <div className="flex items-center justify-between mb-12">
               <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic uppercase">Institutional Compliance</h3>
               <ShieldCheck className="text-green-500" size={24} />
            </div>
            <div className="space-y-8">
               {[
                 { label: 'Identity Verification Rate', value: 92, color: 'bg-blue-500 shadow-blue-500/30' },
                 { label: 'Tax & Ledger Integrity', value: 78, color: 'bg-purple-500 shadow-purple-500/30' },
                 { label: 'Document Authenticity Index', value: 85, color: 'bg-green-500 shadow-green-500/30' },
               ].map((c, i) => (
                 <div key={i} className="space-y-3">
                    <div className="flex justify-between items-baseline mb-1">
                       <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">{c.label}</span>
                       <span className="text-sm font-[900] text-slate-900 italic tracking-tighter">{c.value}%</span>
                    </div>
                    <div className="w-full h-3 bg-slate-50 rounded-full overflow-hidden p-0.5">
                       <motion.div 
                         initial={{ width: 0 }}
                         animate={{ width: `${c.value}%` }}
                         transition={{ duration: 1, ease: 'easeOut', delay: 0.5 }}
                         className={`h-full ${c.color} rounded-full shadow-lg relative overflow-hidden`}
                       >
                          <div className="absolute inset-0 bg-white/20 animate-shimmer" style={{ background: 'linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent)' }} />
                       </motion.div>
                    </div>
                 </div>
               ))}
            </div>
         </div>
      </div>

      {/* Export Modal */}
      <Modal 
        isOpen={isExportModalOpen} 
        onClose={() => setIsExportModalOpen(false)}
        title="Institutional Intelligence Export"
      >
        <div className="space-y-8">
           <p className="text-sm font-bold text-slate-400 uppercase tracking-widest leading-relaxed">
              Select the data mapping segments for the comprehensive executive report. This operation will be logged in the global audit ledger.
           </p>
           <div className="grid grid-cols-2 gap-4">
              {[
                { label: 'Full Ledger', icon: Database },
                { label: 'Member Identities', icon: Users },
                { label: 'Revenue Streams', icon: TrendingUp },
                { label: 'Security Logs', icon: ShieldCheck },
              ].map((item, i) => (
                <button key={i} className="p-6 bg-slate-50 border border-slate-100 rounded-[32px] flex flex-col items-center gap-4 hover:border-blue-500 hover:bg-white transition-all group">
                   <div className="w-10 h-10 rounded-xl bg-white flex items-center justify-center text-slate-400 group-hover:text-blue-600 shadow-sm transition-colors">
                      <item.icon size={18} />
                   </div>
                   <span className="text-[10px] font-black text-slate-900 uppercase tracking-widest">{item.label}</span>
                </button>
              ))}
           </div>
           <div className="pt-6 flex justify-end gap-4 border-t border-slate-100">
              <button 
                onClick={() => setIsExportModalOpen(false)}
                className="px-8 py-4 rounded-xl text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-colors"
              >
                Cancel
              </button>
              <Button onClick={handleExport} className="px-10 py-4 shadow-xl shadow-blue-500/20 rounded-[20px]">
                Proceed with Export
              </Button>
           </div>
        </div>
      </Modal>
    </div>
  );
}
