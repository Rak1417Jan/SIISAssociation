import React from 'react';
import { motion } from 'framer-motion';
import { 
  TrendingUp, 
  Users, 
  MapPin, 
  Building2, 
  PieChart as PieChartIcon, 
  BarChart2, 
  ArrowUpRight, 
  Download,
  Calendar,
  Globe,
  Zap,
  Activity
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMemberStats, getRevenueStats, getFirmStats, exportAnalyticsReport } from '../services/analyticsService';
import { 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
  AreaChart,
  Area
} from 'recharts';

export default function BusinessAnalytics() {
  const [dateRange, setDateRange] = React.useState('monthly');
  
  const { execute: fetchMemberStats, data: memberData, loading: memberLoading } = useApi(getMemberStats);
  const { execute: fetchRevenueStats, data: revenueData, loading: revenueLoading } = useApi(getRevenueStats);
  const { execute: fetchFirmStats, data: firmData, loading: firmLoading } = useApi(getFirmStats);
  const { execute: doExport } = useApi(exportAnalyticsReport);

  React.useEffect(() => {
    fetchMemberStats(dateRange);
    fetchRevenueStats(dateRange);
    fetchFirmStats();
  }, [dateRange, fetchMemberStats, fetchRevenueStats, fetchFirmStats]);

  const handleExport = async () => {
    const tid = toast.loading('Compiling Executive Report...');
    const res = await doExport('business', { dateRange });
    if (!res.error) {
      toast.success('Report Downloaded', {id: tid});
      // window.open(res.data.downloadUrl);
    } else {
      toast.error('Export failed', {id: tid});
    }
  };

  const revenueStats = revenueData?.data;
  const memberStats = memberData?.data;
  const firmStats = firmData?.data;

  // Format data for Recharts
  const revChartData = revenueStats?.labels?.map((label, i) => ({
    month: label,
    revenue: revenueStats.revenue[i],
    users: memberStats?.newRegistrations?.[i] || 0
  })) || [];

  const segmentChartData = [
    { name: 'Elite (Lifetime)', value: revenueStats?.lifetimePlanSales?.reduce((a,b)=>a+b, 0) || 0, color: '#2563eb' },
    { name: 'Standard (Yearly)', value: revenueStats?.yearlyPlanSales?.reduce((a,b)=>a+b, 0) || 0, color: '#7c3aed' },
  ];

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">BUSINESS INTELLIGENCE</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Macro-level data aggregation & market IQ</p>
        </div>
        <div className="flex items-center gap-3">
          <select 
            value={dateRange}
            onChange={e => setDateRange(e.target.value)}
            className="bg-slate-50 border border-slate-200 text-slate-700 text-xs font-bold rounded-xl px-4 py-2 outline-none"
          >
             <option value="weekly">Last 7 Days</option>
             <option value="monthly">Last 6 Months</option>
             <option value="yearly">Fiscal 2026</option>
          </select>
          <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20 active:scale-95" onClick={handleExport}>
            <Download size={14} />
            Executive Report
          </Button>
        </div>
      </div>

      {/* Analytics Macro Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
        {/* Growth Velocity */}
        <div className="xl:col-span-2 glass-panel p-10">
          <div className="flex items-center justify-between mb-10">
             <h3 className="text-xl font-[900] text-slate-900 tracking-tight uppercase italic">Growth Projection</h3>
             <div className="flex items-center gap-4">
                <div className="flex flex-col text-right">
                   <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Global Drift</p>
                   <p className="text-sm font-[900] text-green-500 tracking-tighter italic">+18.4% YoY</p>
                </div>
                <div className="w-12 h-12 rounded-2xl bg-slate-50 flex items-center justify-center text-blue-600">
                   <Activity size={24} />
                </div>
             </div>
          </div>
          <div className="h-[350px] w-full flex items-center justify-center">
            {revenueLoading || memberLoading ? (
               <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
            ) : revChartData.length === 0 ? (
               <p className="text-xs font-bold text-slate-400">No data for selected range</p>
            ) : (
               <ResponsiveContainer width="100%" height="100%">
                 <AreaChart data={revChartData}>
                   <defs>
                     <linearGradient id="colorRev" x1="0" y1="0" x2="0" y2="1">
                       <stop offset="5%" stopColor="#2563eb" stopOpacity={0.1}/>
                       <stop offset="95%" stopColor="#2563eb" stopOpacity={0}/>
                     </linearGradient>
                   </defs>
                   <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                   <XAxis dataKey="month" axisLine={false} tickLine={false} tick={{fontSize: 10, fontWeight: 900, fill:'#94a3b8'}} />
                   <YAxis axisLine={false} tickLine={false} tick={{fontSize: 10, fontWeight: 900, fill:'#94a3b8'}} />
                   <Tooltip contentStyle={{borderRadius:'16px', border:'none', boxShadow:'0 20px 40px -10px rgba(0,0,0,0.1)'}} />
                   <Area type="monotone" dataKey="revenue" stroke="#2563eb" strokeWidth={4} fillOpacity={1} fill="url(#colorRev)" />
                   <Area type="monotone" dataKey="users" stroke="#7c3aed" strokeWidth={2} strokeDasharray="5 5" fillOpacity={0} />
                 </AreaChart>
               </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Member Composition */}
        <div className="xl:col-span-1 glass-panel p-10 flex flex-col justify-between">
           <div>
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-10">Member Composition</h3>
              <div className="h-[240px] w-full flex items-center justify-center">
                {revenueLoading ? (
                   <div className="w-8 h-8 border-4 border-purple-600 border-t-transparent rounded-full animate-spin" />
                ) : (
                   <ResponsiveContainer width="100%" height="100%">
                     <PieChart>
                       <Pie
                         data={segmentChartData}
                         innerRadius={60}
                         outerRadius={80}
                         paddingAngle={8}
                         dataKey="value"
                       >
                         {segmentChartData.map((entry, index) => (
                           <Cell key={`cell-${index}`} fill={entry.color} />
                         ))}
                       </Pie>
                       <Tooltip contentStyle={{borderRadius:'16px', border:'none'}} />
                     </PieChart>
                   </ResponsiveContainer>
                )}
              </div>
              <div className="space-y-4 mt-8">
                 {segmentChartData.map((s, i) => (
                   <div key={i} className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                         <div className="w-2 h-2 rounded-full" style={{ backgroundColor: s.color }} />
                         <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{s.name} Sales</span>
                      </div>
                      <span className="text-xs font-[900] text-slate-900 italic">{s.value}</span>
                   </div>
                 ))}
              </div>
           </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
         <div className="glass-panel p-10 bg-slate-900 text-white overflow-hidden relative">
            <div className="absolute top-0 right-0 w-80 h-80 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
            <div className="flex flex-col h-full justify-between relative z-10">
               <div>
                  <Globe className="text-blue-500 mb-8" size={40} />
                  <h3 className="text-3xl font-[900] tracking-tighter mb-4 italic leading-tight uppercase">REGIONAL<br />SATURATION INDEX</h3>
                  <p className="text-slate-400 font-bold text-xs uppercase tracking-widest leading-relaxed max-w-sm">
                     The Western Hub is now operating at 94% institutional capacity.
                  </p>
               </div>
               <div className="mt-12 flex items-center gap-8">
                  <div className="flex flex-col">
                     <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Total Firms</span>
                     <span className="text-xl font-[900] text-white tracking-tighter italic">{firmLoading ? '...' : firmStats?.totalFirms || 0}</span>
                  </div>
                  <div className="w-[1px] h-10 bg-white/10" />
                  <div className="flex flex-col">
                     <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Active Firms</span>
                     <span className="text-xl font-[900] text-blue-400 tracking-tighter italic">{firmLoading ? '...' : firmStats?.activeFirms || 0}</span>
                  </div>
               </div>
            </div>
         </div>

         <div className="glass-panel p-10 flex flex-col justify-between">
            <div>
               <div className="flex items-center justify-between mb-8">
                  <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">Performance Benchmarks</h3>
                  <Zap className="text-amber-500" size={18} />
               </div>
               <div className="space-y-6">
                  {[
                    { label: 'Avg Members / Firm', value: firmStats?.avgMembersPerFirm || 0, drift: 'Stable' },
                    { label: 'Top Firm: ' + (firmStats?.topFirms?.[0]?.name || 'N/A'), value: firmStats?.topFirms?.[0]?.memberCount || 0, drift: 'Members' },
                    { label: 'Top Firm: ' + (firmStats?.topFirms?.[1]?.name || 'N/A'), value: firmStats?.topFirms?.[1]?.memberCount || 0, drift: 'Members' },
                  ].map((b, i) => (
                    <div key={i} className="flex justify-between items-end p-5 bg-slate-50/50 rounded-2xl border border-slate-100">
                       <div>
                          <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">{b.label}</p>
                          <p className="text-2xl font-[900] text-slate-900 tracking-tighter italic uppercase">{firmLoading ? '...' : b.value}</p>
                       </div>
                       <span className="text-[9px] font-black text-blue-600 uppercase tracking-widest mb-1">{b.drift}</span>
                    </div>
                  ))}
               </div>
            </div>
         </div>
      </div>
    </div>
  );
}
