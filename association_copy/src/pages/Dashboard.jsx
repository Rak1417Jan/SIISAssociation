import React from 'react';
import { motion } from 'framer-motion';
import { 
  Users, 
  TrendingUp, 
  Building2, 
  Clock, 
  ArrowUpRight, 
  ArrowDownRight,
  UserPlus,
  CheckCircle,
  XCircle,
  AlertCircle,
  FileText,
  CreditCard,
  ChevronRight,
  Zap,
  MoreVertical
} from 'lucide-react';
import { Badge } from '../components/ui/Badge';
import { Button } from '../components/ui/Button';
import { useCountUp } from '../hooks/useCountUp';
import { useNavigate } from 'react-router-dom';
import {
  AreaChart, Area, BarChart, Bar, XAxis, YAxis, Tooltip,
  ResponsiveContainer, CartesianGrid
} from 'recharts';

const areaData = [
  { month: 'Oct', revenue: 42000 }, { month: 'Nov', revenue: 58000 },
  { month: 'Dec', revenue: 47000 }, { month: 'Jan', revenue: 71000 },
  { month: 'Feb', revenue: 65000 }, { month: 'Mar', revenue: 91000 },
  { month: 'Apr', revenue: 82000 },
];

const barData = [
  { month: 'Oct', members: 28 }, { month: 'Nov', members: 42 },
  { month: 'Dec', members: 35 }, { month: 'Jan', members: 58 },
  { month: 'Feb', members: 47 }, { month: 'Mar', members: 73 },
  { month: 'Apr', members: 61 },
];

const activities = [
  { icon: UserPlus, color: 'text-blue-600 bg-blue-50', text: 'New member registered', user: 'Daksh Sharma', time: '2 min ago' },
  { icon: CheckCircle, color: 'text-green-600 bg-green-50', text: 'Payment confirmed', user: 'Anita Shah', amount: '₹2,000', time: '15 min ago' },
  { icon: XCircle, color: 'text-red-600 bg-red-50', text: 'Application rejected', user: 'Incomplete docs', time: '1h ago' },
  { icon: AlertCircle, color: 'text-amber-600 bg-amber-50', text: 'Renewal reminder sent', user: '12 members', time: '3h ago' },
];

function KPICard({ title, value, icon: Icon, trend, color, suffix = '', delay = 0 }) {
  const count = useCountUp(value);
  const isPositive = trend.startsWith('↑');
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay }}
      className="glass-panel p-6 overflow-hidden relative group"
    >
      <div className={`absolute -right-4 -top-4 w-24 h-24 rounded-full opacity-[0.03] group-hover:scale-150 transition-transform duration-700 ${color}`} />
      
      <div className="flex items-center justify-between mb-4">
        <div className={`w-12 h-12 rounded-2xl flex items-center justify-center ${color} bg-opacity-10 text-current`}>
          <Icon size={22} className={`${color.replace('bg-', 'text-')}`} />
        </div>
        <button className="text-slate-300 hover:text-slate-600 transition-colors">
          <MoreVertical size={18} />
        </button>
      </div>

      <div>
        <p className="text-xs font-black text-slate-400 uppercase tracking-widest mb-1">{title}</p>
        <h3 className="text-3xl font-black text-slate-900 tracking-tighter">
          {suffix}{count.toLocaleString()}
        </h3>
        <div className="flex items-center gap-2 mt-3">
          <div className={`flex items-center text-[10px] font-black px-2 py-0.5 rounded-full ${isPositive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
            {isPositive ? <ArrowUpRight size={10} className="mr-0.5" /> : <ArrowDownRight size={10} className="mr-0.5" />}
            {trend.split(' ')[1]}
          </div>
          <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{trend.split(' ').slice(2).join(' ')}</span>
        </div>
      </div>
    </motion.div>
  );
}

const CustomTooltip = ({ active, payload, label }) => {
  if (!active || !payload?.length) return null;
  return (
    <div className="glass-panel p-4 shadow-2xl border-slate-100 min-w-[140px]">
      <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">{label}</p>
      {payload.map((p, i) => (
        <div key={i} className="flex items-center justify-between gap-4">
          <span className="text-xs font-bold text-slate-600">{p.name}</span>
          <span className="text-sm font-black text-blue-600">₹{(p.value / 1000).toFixed(1)}k</span>
        </div>
      ))}
    </div>
  );
};

export default function Dashboard() {
  const navigate = useNavigate();

  return (
    <div className="space-y-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-black text-slate-900 tracking-tighter italic">DASHBOARD</h1>
          <div className="flex items-center gap-2 mt-2">
            <Badge status="active" label="System Active" />
            <span className="text-[10px] font-black text-slate-300 uppercase tracking-widest">Last updated 2 mins ago</span>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" className="rounded-2xl px-6 font-black text-xs uppercase tracking-widest flex items-center gap-2">
            <FileText size={16} />
            Reports
          </Button>
          <Button onClick={() => navigate('/register')} className="rounded-2xl px-8 py-3.5 btn-premium shadow-xl shadow-blue-500/20 font-black text-xs uppercase tracking-widest flex items-center gap-2">
            <UserPlus size={16} />
            New Member
          </Button>
        </div>
      </div>

      {/* KPI Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <KPICard title="Total Members" value={2418} icon={Users} trend="↑ 12% vs last month" color="bg-blue-600" />
        <KPICard title="Revenue Pool" value={82400} icon={TrendingUp} trend="↑ 8.4% vs last month" color="bg-green-600" suffix="₹" delay={0.1} />
        <KPICard title="Registered Firms" value={347} icon={Building2} trend="↑ 5.2% new firms" color="bg-indigo-600" delay={0.2} />
        <KPICard title="Pending Reviews" value={28} icon={Clock} trend="↓ 2 from yesterday" color="bg-amber-600" delay={0.3} />
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
        <div className="xl:col-span-2 glass-panel p-8">
          <div className="flex items-center justify-between mb-10">
            <div>
              <h3 className="text-xl font-black text-slate-900 tracking-tight">Revenue Analytics</h3>
              <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Performance trend over 6 months</p>
            </div>
            <div className="flex gap-2">
              {['7D', '1M', '6M', '1Y'].map(t => (
                <button key={t} className={`px-3 py-1.5 rounded-xl text-[10px] font-black transition-all ${t === '6M' ? 'bg-slate-900 text-white' : 'text-slate-400 hover:bg-slate-100'}`}>
                  {t}
                </button>
              ))}
            </div>
          </div>
          
          <div className="h-[300px] w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={areaData}>
                <defs>
                  <linearGradient id="areaGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#2563eb" stopOpacity={0.1} />
                    <stop offset="95%" stopColor="#2563eb" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                <XAxis 
                  dataKey="month" 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{ fontSize: 10, fontWeight: 800, fill: '#94a3b8' }} 
                  dy={10}
                />
                <YAxis 
                  axisLine={false} 
                  tickLine={false} 
                  tick={{ fontSize: 10, fontWeight: 800, fill: '#94a3b8' }} 
                  tickFormatter={v => `₹${v/1000}k`}
                />
                <Tooltip content={<CustomTooltip />} />
                <Area 
                  type="monotone" 
                  dataKey="revenue" 
                  stroke="#2563eb" 
                  strokeWidth={4} 
                  fillOpacity={1} 
                  fill="url(#areaGradient)" 
                  animationDuration={2000}
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="glass-panel p-8">
           <h3 className="text-xl font-black text-slate-900 tracking-tight mb-8">Quick Actions</h3>
           <div className="space-y-4">
              {[
                { label: 'Register New Firm', icon: Building2, color: 'text-blue-600 bg-blue-50' },
                { label: 'Record New Payment', icon: CreditCard, color: 'text-green-600 bg-green-50' },
                { label: 'Generate ID Card', icon: Zap, color: 'text-purple-600 bg-purple-50' },
                { label: 'System Settings', icon: Settings, color: 'text-slate-600 bg-slate-50' },
              ].map((action, i) => (
                <motion.button
                  key={i}
                  whileHover={{ scale: 1.02, x: 5 }}
                  whileTap={{ scale: 0.98 }}
                  className="w-full flex items-center justify-between p-4 rounded-2xl bg-slate-50/50 border border-slate-100 hover:border-blue-200 transition-all group"
                >
                  <div className="flex items-center gap-4">
                    <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${action.color}`}>
                      <action.icon size={18} />
                    </div>
                    <span className="text-xs font-black text-slate-700 uppercase tracking-widest">{action.label}</span>
                  </div>
                  <ChevronRight size={16} className="text-slate-300 group-hover:text-blue-600 transition-colors" />
                </motion.button>
              ))}
           </div>
           
           <div className="mt-8 p-5 bg-gradient-to-br from-slate-900 to-slate-800 rounded-[32px] text-white relative overflow-hidden">
             <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/10 blur-3xl rounded-full" />
             <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-2">Platform Status</p>
             <div className="flex items-end justify-between">
               <div>
                 <p className="text-2xl font-black italic">UPTIME</p>
                 <p className="text-sm font-bold text-green-400 mt-1">99.98% Healthy</p>
               </div>
               <div className="flex gap-1 h-8 items-end">
                 {[12, 18, 14, 22, 16, 24, 20].map((h, i) => (
                   <div key={i} className="w-1 bg-green-500/30 rounded-t-full" style={{ height: h }} />
                 ))}
               </div>
             </div>
           </div>
        </div>
      </div>

      {/* Activity Timeline */}
      <div className="glass-panel p-8">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h3 className="text-xl font-black text-slate-900 tracking-tight">Recent Activity</h3>
            <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Real-time system logs</p>
          </div>
          <button className="text-xs font-black text-blue-600 hover:text-blue-700 uppercase tracking-widest">View All Logs</button>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-x-12 gap-y-6">
          {activities.map((act, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: i * 0.1 }}
              className="flex items-center gap-5 p-4 rounded-3xl hover:bg-slate-50/80 transition-all border border-transparent hover:border-slate-100 group"
            >
              <div className={`w-14 h-14 rounded-2xl flex items-center justify-center shrink-0 ${act.color} group-hover:scale-110 transition-transform`}>
                <act.icon size={24} />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-black text-slate-900 tracking-tight">{act.text}</p>
                  <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{act.time}</span>
                </div>
                <div className="flex items-center justify-between mt-1">
                  <p className="text-xs font-bold text-slate-500 truncate">{act.user}</p>
                  {act.amount && <span className="text-xs font-black text-green-600">{act.amount}</span>}
                </div>
              </div>
              <button className="p-2 text-slate-200 hover:text-slate-400 transition-colors opacity-0 group-hover:opacity-100">
                <ChevronRight size={18} />
              </button>
            </motion.div>
          ))}
        </div>
      </div>
    </div>
  );
}
