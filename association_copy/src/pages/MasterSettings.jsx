import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Settings, 
  ShieldCheck, 
  Globe, 
  Database, 
  Lock, 
  Bell, 
  Smartphone, 
  Cloud,
  ChevronRight,
  Zap,
  Save,
  Terminal,
  Activity,
  Server,
  Key,
  ShieldAlert,
  Cpu
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getSettings, updateSettings, uploadLogo } from '../services/settingsService';

export default function MasterSettings() {
  const [activeTab, setActiveTab] = useState('institutional');
  const [loading, setLoading] = useState(false);
  const { execute: fetchSettings, data: settingsData } = useApi(getSettings);
  const { execute: saveSettings } = useApi(updateSettings);
  const { execute: doUploadLogo } = useApi(uploadLogo);
  
  const [formData, setFormData] = useState({});

  React.useEffect(() => {
    fetchSettings().then(res => {
      if (!res.error && res.data?.data) {
         setFormData(res.data.data);
      }
    });
  }, [fetchSettings]);
  
  const [multiRegionSync, setMultiRegionSync] = useState(true);
  
  const [securitySettings, setSecuritySettings] = useState([
    { icon: Key, title: '2FA Enforcement', desc: 'Require TOTP for all Level 5+ administrative personnel.', active: true },
    { icon: ShieldAlert, title: 'Institutional Shield', desc: 'Block access from non-verified institutional IP ranges.', active: false },
    { icon: Lock, title: 'Session Hardening', desc: 'Enforce 15-minute inactivity timeout across all nodes.', active: true },
  ]);

  const [archiveSchedule, setArchiveSchedule] = useState('Real-time');

  const handleSync = async () => {
    setLoading(true);
    const res = await saveSettings(formData);
    if (!res.error) {
      toast.success('Core systems synchronized across all regional nodes');
    } else {
      toast.error('Failed to synchronize');
    }
    setLoading(false);
  };

  const handleRevert = () => {
    toast('Reverting to institutional defaults...', { icon: '🔄' });
    setTimeout(() => {
      fetchSettings().then(res => {
         if (!res.error && res.data?.data) {
            setFormData(res.data.data);
            toast.success('Defaults restored successfully');
         }
      });
    }, 1000);
  };

  const handleLogoUpload = async (e) => {
    const file = e.target.files?.[0];
    if (file) {
      const tid = toast.loading('Uploading logo...');
      const res = await doUploadLogo(file);
      if (!res.error) {
        setFormData(prev => ({ ...prev, logo: res.data.logoUrl }));
        toast.success('Logo updated', { id: tid });
      } else {
        toast.error('Logo upload failed', { id: tid });
      }
    }
  };

  const updateField = (key, value) => setFormData(prev => ({ ...prev, [key]: value }));

  const toggleSecurity = (index) => {
    const newSettings = [...securitySettings];
    newSettings[index].active = !newSettings[index].active;
    setSecuritySettings(newSettings);
    toast.success(`${newSettings[index].title} ${newSettings[index].active ? 'Enabled' : 'Disabled'}`);
  };

  const tabs = [
    { id: 'institutional', label: 'Institutional Config', icon: Globe },
    { id: 'security', label: 'Security Baseline', icon: ShieldCheck },
    { id: 'database', label: 'Database & Sync', icon: Database },
    { id: 'notifications', label: 'Alert Protocol', icon: Bell },
    { id: 'terminal', label: 'System Terminal', icon: Terminal },
  ];

  const renderContent = () => {
    switch (activeTab) {
      case 'institutional':
        return (
          <motion.div 
            initial={{ opacity: 0, y: 10 }} 
            animate={{ opacity: 1, y: 0 }} 
            className="space-y-10"
          >
             <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Institutional Name</label>
                   <input type="text" value={formData.associationName || ''} onChange={e => updateField('associationName', e.target.value)} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">HQ Address</label>
                   <input type="text" value={formData.address || ''} onChange={e => updateField('address', e.target.value)} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
             </div>
             
             <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Support Email</label>
                   <input type="email" value={formData.supportEmail || ''} onChange={e => updateField('supportEmail', e.target.value)} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Support Phone</label>
                   <input type="text" value={formData.supportPhone || ''} onChange={e => updateField('supportPhone', e.target.value)} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
             </div>

             <div className="grid grid-cols-1 md:grid-cols-3 gap-10">
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Yearly Fee</label>
                   <input type="number" value={formData.yearlyFee || 0} onChange={e => updateField('yearlyFee', Number(e.target.value))} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Lifetime Fee</label>
                   <input type="number" value={formData.lifetimeFee || 0} onChange={e => updateField('lifetimeFee', Number(e.target.value))} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
                <div className="space-y-4">
                   <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">GST %</label>
                   <input type="number" value={formData.gstPercent || 0} onChange={e => updateField('gstPercent', Number(e.target.value))} className="w-full px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                </div>
             </div>
             
             <div className="p-6 bg-slate-50/50 rounded-2xl border border-slate-100">
                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-4">Live Payment Breakdown (Yearly)</p>
                <div className="flex justify-between items-center text-xs font-bold text-slate-800">
                   <span>Base: ₹{formData.yearlyFee || 0}</span>
                   <span>GST: ₹{((formData.yearlyFee || 0) * (formData.gstPercent || 0) / 100).toFixed(2)}</span>
                   <span>Platform Fee: ₹{formData.platformFeeFlat || 0}</span>
                   <span className="text-blue-600">Total: ₹{((formData.yearlyFee || 0) + ((formData.yearlyFee || 0) * (formData.gstPercent || 0) / 100) + (formData.platformFeeFlat || 0)).toFixed(2)}</span>
                </div>
             </div>

             <div className="space-y-4">
                <label className="text-[10px] font-black text-slate-900 uppercase tracking-widest ml-1">Logo URL / Upload</label>
                <div className="flex gap-4 items-center">
                  <input type="text" value={formData.logo || ''} onChange={e => updateField('logo', e.target.value)} className="flex-1 px-6 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-xs font-black outline-none focus:border-blue-500 transition-all" />
                  <input type="file" id="logoUpload" className="hidden" onChange={handleLogoUpload} accept="image/*" />
                  <label htmlFor="logoUpload" className="px-6 py-4 bg-blue-50 text-blue-600 rounded-2xl text-xs font-black cursor-pointer hover:bg-blue-100 transition-colors uppercase tracking-widest">
                     Upload
                  </label>
                </div>
                {formData.logo && <img src={formData.logo} alt="Logo Preview" className="h-16 mt-4 object-contain" />}
             </div>
             <div className="p-8 bg-blue-50/50 border border-blue-100 rounded-[32px] flex items-center justify-between">
                <div className="flex items-center gap-4">
                   <div className="w-12 h-12 bg-white rounded-xl flex items-center justify-center text-blue-600 shadow-sm">
                      <Globe size={24} />
                   </div>
                   <div>
                      <p className="text-xs font-black text-slate-900 uppercase tracking-widest">Multi-Region Synchronization</p>
                      <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Propagation delay: 124ms</p>
                   </div>
                </div>
                <button 
                  onClick={() => {
                    setMultiRegionSync(!multiRegionSync);
                    toast.success(`Multi-Region Sync ${!multiRegionSync ? 'Enabled' : 'Disabled'}`);
                  }}
                  className={`w-12 h-6 rounded-full relative p-1 transition-all ${multiRegionSync ? 'bg-blue-600' : 'bg-slate-300'}`}
                >
                   <div className={`h-4 w-4 bg-white rounded-full absolute top-1 transition-all ${multiRegionSync ? 'right-1' : 'left-1'}`} />
                </button>
             </div>
          </motion.div>
        );
      case 'security':
        return (
          <motion.div 
             initial={{ opacity: 0, y: 10 }} 
             animate={{ opacity: 1, y: 0 }} 
             className="space-y-8"
          >
             {securitySettings.map((item, i) => (
                <div key={i} onClick={() => toggleSecurity(i)} className="p-8 bg-slate-50 rounded-[32px] border border-slate-100 flex items-center justify-between hover:bg-white hover:shadow-xl transition-all group cursor-pointer active:scale-[0.99]">
                   <div className="flex items-center gap-6">
                      <div className="w-12 h-12 bg-white rounded-2xl flex items-center justify-center text-slate-400 group-hover:text-blue-600 shadow-sm transition-colors">
                         <item.icon size={22} />
                      </div>
                      <div>
                         <p className="text-sm font-[900] text-slate-900 tracking-tight italic uppercase">{item.title}</p>
                         <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mt-1">{item.desc}</p>
                      </div>
                   </div>
                   <button className={`w-12 h-6 rounded-full relative p-1 transition-all ${item.active ? 'bg-blue-600' : 'bg-slate-300'}`}>
                      <div className={`h-4 w-4 bg-white rounded-full absolute top-1 transition-all ${item.active ? 'right-1' : 'left-1'}`} />
                   </button>
                </div>
             ))}
          </motion.div>
        );
      case 'database':
        return (
          <motion.div 
             initial={{ opacity: 0, y: 10 }} 
             animate={{ opacity: 1, y: 0 }} 
             className="space-y-8"
          >
             <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="p-10 bg-slate-900 rounded-[40px] text-white relative overflow-hidden group hover:shadow-2xl hover:shadow-blue-500/20 transition-all cursor-pointer" onClick={() => toast('Ping: 12ms', {icon: '📶'})}>
                   <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/20 blur-3xl rounded-full" />
                   <div className="relative z-10">
                      <div className="flex items-center gap-4 mb-8">
                         <Server className="text-blue-500" size={24} />
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-400">Primary Core Node</span>
                      </div>
                      <h4 className="text-4xl font-[900] tracking-tighter italic">ALPHA-S1</h4>
                      <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mt-4">Status: Operational</p>
                   </div>
                </div>
                <div className="p-10 bg-white border-2 border-slate-50 rounded-[40px] relative overflow-hidden group hover:border-blue-200 transition-all cursor-pointer" onClick={() => toast('Force Sync Initiated', {icon: '🔄'})}>
                   <div className="relative z-10">
                      <div className="flex items-center gap-4 mb-8">
                         <Cpu className="text-purple-600" size={24} />
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-400">Redundancy Cluster</span>
                      </div>
                      <h4 className="text-4xl font-[900] text-slate-900 tracking-tighter italic uppercase">Beta-Backup</h4>
                      <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest mt-4">Last Sync: 2m ago</p>
                   </div>
                </div>
             </div>
             <div className="p-8 bg-slate-50 border border-slate-100 rounded-[32px]">
                <h5 className="text-[10px] font-black text-slate-900 uppercase tracking-[0.2em] mb-6">Archive Schedule</h5>
                <div className="flex bg-white p-2 rounded-2xl border border-slate-100">
                   {['Daily', 'Weekly', 'Real-time'].map(s => (
                     <button 
                       key={s} 
                       onClick={() => {
                         setArchiveSchedule(s);
                         toast.success(`Archive schedule set to ${s}`);
                       }}
                       className={`flex-1 py-3 rounded-xl text-[10px] font-black uppercase tracking-widest transition-all ${archiveSchedule === s ? 'bg-blue-600 text-white shadow-lg' : 'text-slate-400 hover:text-slate-900'}`}
                     >
                        {s}
                     </button>
                   ))}
                </div>
             </div>
          </motion.div>
        );
      case 'notifications':
        return (
          <motion.div 
             initial={{ opacity: 0, y: 10 }} 
             animate={{ opacity: 1, y: 0 }} 
             className="space-y-8"
          >
             <h4 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase mb-4">Event Subscription Matrix</h4>
             <div className="divide-y divide-slate-100 border border-slate-100 rounded-[32px] overflow-hidden">
                {[
                  { label: 'Security Breaches', sms: true, push: true, email: true },
                  { label: 'New Member Onboarding', sms: false, push: true, email: true },
                  { label: 'System Maintenance', sms: true, push: false, email: true },
                  { label: 'Financial Transactions', sms: true, push: true, email: false },
                ].map((row, i) => (
                  <div key={i} className="p-6 bg-white flex items-center justify-between px-10 hover:bg-slate-50 transition-colors cursor-pointer" onClick={() => toast.success(`Updated routing for ${row.label}`)}>
                    <span className="text-xs font-black text-slate-800 uppercase tracking-widest">{row.label}</span>
                    <div className="flex gap-10">
                       <div className="flex items-center gap-3">
                          <div className={`w-4 h-4 rounded border-2 ${row.push ? 'bg-blue-600 border-blue-600' : 'bg-white border-slate-200'}`} />
                          <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Push</span>
                       </div>
                       <div className="flex items-center gap-3">
                          <div className={`w-4 h-4 rounded border-2 ${row.sms ? 'bg-blue-600 border-blue-600' : 'bg-white border-slate-200'}`} />
                          <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">SMS</span>
                       </div>
                    </div>
                  </div>
                ))}
             </div>
          </motion.div>
        );
      case 'terminal':
        return (
          <motion.div 
             initial={{ opacity: 0, y: 10 }} 
             animate={{ opacity: 1, y: 0 }} 
             className="bg-slate-900 rounded-[40px] p-10 font-mono text-xs text-blue-400 relative overflow-hidden cursor-text"
             onClick={() => toast('Terminal interface is view-only for security', { icon: '🛡️' })}
          >
             <div className="absolute top-4 right-6 flex gap-2">
                <div className="w-2 h-2 rounded-full bg-red-500/50" />
                <div className="w-2 h-2 rounded-full bg-amber-500/50" />
                <div className="w-2 h-2 rounded-full bg-green-500/50" />
             </div>
             <div className="space-y-3 opacity-80">
                <p className="text-slate-500"># Institutional Core Node Alpha-S1 Boot Sequence...</p>
                <p><span className="text-blue-600">[OK]</span> Network Stack: Global Mesh Active</p>
                <p><span className="text-blue-600">[OK]</span> Security Kernel: Institutional V4 Loaded</p>
                <p><span className="text-amber-500">[WARN]</span> Latency spike detected in Southern Zone</p>
                <p><span className="text-blue-600">[OK]</span> DB Sync: Consistent with Beta-Backup</p>
                <p className="mt-6 flex items-center gap-2">
                   <span className="text-green-500">$</span>
                   <span className="w-2 h-4 bg-green-500 animate-pulse" />
                </p>
             </div>
          </motion.div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">CORE CONTROL</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional system configuration & security</p>
        </div>
        <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20" onClick={handleSync} disabled={loading}>
          {loading ? <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : <Save size={16} />}
          Commit All Changes
        </Button>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-4 gap-10">
        {/* Navigation Sidebar */}
        <div className="xl:col-span-1 space-y-4">
           {tabs.map(tab => (
             <button
               key={tab.id}
               onClick={() => setActiveTab(tab.id)}
               className={`w-full p-6 rounded-[24px] flex items-center justify-between transition-all duration-300
                 ${activeTab === tab.id 
                   ? 'bg-white shadow-[0_20px_40px_-10px_rgba(59,130,246,0.1)] border border-slate-50' 
                   : 'bg-transparent border border-transparent text-slate-400 hover:bg-slate-50'}
               `}
             >
                <div className="flex items-center gap-5">
                   <div className={`w-12 h-12 rounded-xl flex items-center justify-center transition-all duration-500
                     ${activeTab === tab.id ? 'bg-blue-600 text-white shadow-xl shadow-blue-600/30 rotate-3' : 'bg-slate-100 text-slate-400'}
                   `}>
                      <tab.icon size={20} />
                   </div>
                   <span className={`text-[10px] font-black uppercase tracking-widest ${activeTab === tab.id ? 'text-slate-900' : ''}`}>
                      {tab.label}
                   </span>
                </div>
                {activeTab === tab.id && <ChevronRight size={14} className="text-blue-600" />}
             </button>
           ))}

           <div className="mt-10 p-10 glass-panel bg-slate-900 text-white relative overflow-hidden group">
              <div className="absolute top-0 right-0 w-32 h-32 bg-blue-600/20 blur-3xl rounded-full" />
              <div className="relative z-10 flex flex-col justify-between h-full">
                 <div>
                    <p className="text-[10px] font-black uppercase tracking-widest text-slate-500 mb-6">Cloud Status</p>
                    <div className="flex items-center justify-between">
                       <p className="text-2xl font-[900] italic">99.98% OPS</p>
                       <Activity size={24} className="text-green-500 animate-pulse" />
                    </div>
                 </div>
                 <div className="mt-8 flex items-center gap-2">
                    <div className="w-1.5 h-1.5 bg-green-500 rounded-full" />
                    <span className="text-[9px] font-black uppercase tracking-widest text-slate-500 italic">Zone: Mumbai-Alpha</span>
                 </div>
              </div>
           </div>
        </div>

        {/* Content Area */}
        <div className="xl:col-span-3">
           <div className="glass-panel p-10 lg:p-14 relative overflow-hidden">
              <div className="absolute top-[-20%] right-[-10%] w-96 h-96 bg-blue-500/5 blur-[120px] rounded-full pointer-events-none" />
              
              <div className="mb-12 flex items-center justify-between relative z-10">
                 <div>
                    <h3 className="text-2xl font-[900] text-slate-900 tracking-tight italic uppercase">
                       {activeTab.replace('_', ' ')} settings
                    </h3>
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1 italic">
                       Modify the institutional baseline for this system module
                    </p>
                 </div>
                 <div className="flex items-center gap-3">
                    <span className="text-[9px] font-black text-blue-600 uppercase tracking-widest bg-blue-50 px-4 py-2 rounded-xl">Alpha V4.0</span>
                 </div>
              </div>

              <div className="relative z-10 min-h-[400px]">
                 {renderContent()}
              </div>

              <div className="mt-16 pt-10 border-t border-slate-50 flex justify-end gap-4 relative z-10">
                 <button onClick={handleRevert} className="px-8 py-4 rounded-[14px] text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-colors active:scale-95">
                    Revert Defaults
                 </button>
                 <Button onClick={handleSync} disabled={loading} className="px-10 py-4 rounded-[20px] bg-blue-600 hover:bg-blue-700 text-white font-black text-[10px] uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/20 active:scale-95">
                    Synchronize Cluster
                 </Button>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
