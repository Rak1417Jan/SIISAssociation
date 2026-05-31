import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  ShieldAlert, 
  MessageSquare, 
  History, 
  FileSearch, 
  ArrowRight, 
  Plus, 
  ChevronRight,
  ShieldCheck,
  Zap,
  Clock,
  MoreVertical,
  X,
  Send,
  Loader2
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMemberGrievances, getAllGrievances, submitGrievance, getGrievanceStats, respondToGrievance } from '../services/engagementService';
import PermissionGate from '../components/PermissionGate';
import { usePermissions } from '../hooks/usePermissions';

export default function GrievancePortal() {
  const [activeTab, setActiveTab] = useState('active'); // 'active' (Personal) or 'global' (Admin)
  const [showSubmitForm, setShowSubmitForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  
  // Submit Form State
  const [formData, setFormData] = useState({ category: 'PAYMENT', subject: '', description: '' });
  
  // Admin Response State
  const [adminResponse, setAdminResponse] = useState('');
  const [adminStatus, setAdminStatus] = useState('IN_PROGRESS');

  const { hasPermission } = usePermissions();
  const canManage = hasPermission('manage', 'grievances');

  const { execute: fetchMemberGrievances, data: memberData, loading: loadingMember } = useApi(getMemberGrievances);
  const { execute: fetchAllGrievances, data: allData, loading: loadingAll } = useApi(getAllGrievances);
  const { execute: fetchStats, data: statsData } = useApi(getGrievanceStats);
  const { execute: doSubmit, loading: submitting } = useApi(submitGrievance);
  const { execute: doRespond, loading: responding } = useApi(respondToGrievance);

  useEffect(() => {
    fetchMemberGrievances('CURRENT_USER');
    if (canManage) {
      fetchStats();
      if (activeTab === 'global') {
        fetchAllGrievances();
      }
    }
  }, [activeTab, canManage, fetchMemberGrievances, fetchAllGrievances, fetchStats]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.subject || formData.description.length < 20) {
      toast.error('Subject and a description (min 20 chars) are required');
      return;
    }
    const res = await doSubmit('CURRENT_USER', formData);
    if (!res.error) {
      toast.success('Grievance Submitted');
      setShowSubmitForm(false);
      setFormData({ category: 'PAYMENT', subject: '', description: '' });
      fetchMemberGrievances('CURRENT_USER');
    }
  };

  const handleRespond = async (grievanceId) => {
    if (!adminResponse) {
      toast.error('Response cannot be empty');
      return;
    }
    const res = await doRespond(grievanceId, adminResponse, adminStatus);
    if (!res.error) {
      toast.success(res.data.message);
      setAdminResponse('');
      fetchAllGrievances();
    }
  };

  const grievances = activeTab === 'active' 
    ? (memberData?.data || []) 
    : (allData?.data?.data || []);
  
  const loading = activeTab === 'active' ? loadingMember : loadingAll;
  const stats = statsData?.data || { total: '-', open: '-', resolved: '-', avgResolutionHours: '-' };

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">GRIEVANCE PORTAL</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional dispute resolution & oversight</p>
        </div>
        <div className="flex items-center gap-3">
          <Button onClick={() => setShowSubmitForm(!showSubmitForm)} size="sm" className="gap-2 shadow-xl shadow-red-500/20 bg-red-600 hover:bg-red-700">
            {showSubmitForm ? <X size={16} /> : <Plus size={16} />}
            {showSubmitForm ? 'Cancel' : 'File Grievance'}
          </Button>
        </div>
      </div>

      <AnimatePresence>
        {showSubmitForm && (
          <motion.div initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: 'auto' }} exit={{ opacity: 0, height: 0 }} className="overflow-hidden">
            <div className="glass-panel p-8 bg-slate-900 text-white">
              <h3 className="text-xl font-[900] tracking-tighter italic uppercase mb-6">Submit New Complaint</h3>
              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Category</label>
                    <select 
                      value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})}
                      className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-sm font-bold outline-none"
                    >
                      <option value="PAYMENT" className="text-black">Payment Issue</option>
                      <option value="DIGITAL_ID" className="text-black">Digital ID Issue</option>
                      <option value="APPROVAL" className="text-black">Membership Approval</option>
                      <option value="RENEWAL" className="text-black">Renewal Issue</option>
                      <option value="STAFF_CONDUCT" className="text-black">Staff Conduct</option>
                      <option value="OTHER" className="text-black">Other</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Subject</label>
                    <input 
                      type="text" required value={formData.subject} onChange={e => setFormData({...formData, subject: e.target.value})}
                      className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-sm font-bold outline-none" placeholder="Brief subject"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Description (Min 20 chars)</label>
                  <textarea 
                    required value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})}
                    className="w-full h-32 bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-sm font-bold outline-none resize-none" placeholder="Detailed description of your issue..."
                  />
                </div>
                <div className="flex justify-end">
                  <Button type="submit" disabled={submitting} className="bg-blue-600 hover:bg-blue-700 px-8">
                    {submitting ? <Loader2 size={16} className="animate-spin" /> : 'Submit Grievance'}
                  </Button>
                </div>
              </form>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <PermissionGate action="manage" resource="grievances">
        {/* Trust Intelligence Bar (Admin Only) */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
           {[
             { label: 'Total Tickets', value: stats.total, color: 'text-blue-500' },
             { label: 'Open', value: stats.open, color: 'text-red-500' },
             { label: 'Resolved', value: stats.resolved, color: 'text-green-500' },
             { label: 'Avg Resolution', value: `${stats.avgResolutionHours}h`, color: 'text-amber-500' },
           ].map((stat, i) => (
             <div key={i} className="glass-panel p-6 flex items-center gap-6">
                <div>
                   <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">{stat.label}</p>
                   <span className={`text-2xl font-[900] ${stat.color} tracking-tighter italic uppercase`}>{stat.value}</span>
                </div>
             </div>
           ))}
        </div>
      </PermissionGate>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
        {/* Active Grievances Ledger */}
        <div className="xl:col-span-2 glass-panel p-10 lg:p-14 overflow-hidden border-slate-100 min-h-[500px]">
           <div className="flex flex-col md:flex-row md:items-center justify-between mb-12 gap-6">
              <h3 className="text-2xl font-[900] text-slate-900 tracking-tight italic uppercase">
                {activeTab === 'active' ? 'My Complaints' : 'Global Feed'}
              </h3>
              
              <PermissionGate action="manage" resource="grievances">
                <div className="flex p-1 bg-slate-50 rounded-xl w-fit">
                   <button 
                     onClick={() => setActiveTab('active')}
                     className={`px-6 py-2 rounded-lg text-[9px] font-black uppercase tracking-widest transition-all ${activeTab === 'active' ? 'bg-white text-slate-900 shadow-xl' : 'text-slate-400 hover:text-slate-600'}`}>
                     Personal
                   </button>
                   <button 
                     onClick={() => setActiveTab('global')}
                     className={`px-6 py-2 rounded-lg text-[9px] font-black uppercase tracking-widest transition-all ${activeTab === 'global' ? 'bg-white text-slate-900 shadow-xl' : 'text-slate-400 hover:text-slate-600'}`}>
                     Global Feed
                   </button>
                </div>
              </PermissionGate>
           </div>

           <div className="space-y-6">
              {loading ? (
                <div className="text-center py-20 text-xs font-bold text-slate-400 uppercase tracking-widest">Loading records...</div>
              ) : grievances.length === 0 ? (
                <div className="text-center py-20 text-xs font-bold text-slate-400 uppercase tracking-widest">No complaints found.</div>
              ) : grievances.map((g, i) => (
                <motion.div 
                  key={g.id}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: i * 0.1 }}
                  className="p-8 bg-white border border-slate-50 rounded-[32px] group hover:shadow-xl hover:border-transparent transition-all relative overflow-hidden"
                >
                   {g.priority === 'HIGH' && <div className="absolute top-0 right-10 w-20 h-1.5 bg-red-500 rounded-b-xl" />}
                   
                   <div 
                     className="flex items-start justify-between cursor-pointer" 
                     onClick={() => setExpandedId(expandedId === g.id ? null : g.id)}
                   >
                      <div className="flex gap-6">
                         <div className="w-14 h-14 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors shadow-inner shrink-0">
                            <ShieldAlert size={24} />
                         </div>
                         <div>
                            <div className="flex flex-wrap items-center gap-3 mb-2">
                               <h4 className="text-lg font-[900] text-slate-900 tracking-tight uppercase italic group-hover:text-blue-600 transition-colors">{g.subject}</h4>
                               <Badge status={g.status === 'RESOLVED' || g.status === 'CLOSED' ? 'active' : g.status === 'IN_PROGRESS' ? 'warning' : 'info'} label={g.status} />
                               <span className="text-[9px] font-bold text-slate-500 bg-slate-100 px-2 py-1 rounded-md">{g.category}</span>
                            </div>
                            <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">
                              {g.ticketNo} • {g.submittedAt}
                              {activeTab === 'global' && ` • ${g.memberName} (${g.firmName})`}
                            </p>
                         </div>
                      </div>
                      <div className="text-right hidden sm:block">
                         <p className={`text-[10px] font-black uppercase tracking-widest mb-2 ${g.priority === 'HIGH' ? 'text-red-500' : 'text-slate-300'}`}>{g.priority} Priority</p>
                         <button className="flex items-center gap-2 text-[10px] font-black text-blue-600 uppercase tracking-widest italic group-hover:translate-x-1 transition-transform ml-auto">
                           {expandedId === g.id ? 'Close' : 'View Intel'} <ArrowRight size={14} />
                         </button>
                      </div>
                   </div>

                   {/* Expandable Details */}
                   <AnimatePresence>
                     {expandedId === g.id && (
                       <motion.div initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: 'auto' }} exit={{ opacity: 0, height: 0 }} className="overflow-hidden">
                         <div className="pt-8 mt-6 border-t border-slate-100 space-y-6">
                           <div>
                             <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Description</p>
                             <p className="text-sm font-bold text-slate-700 leading-relaxed bg-slate-50 p-4 rounded-xl">{g.description || "No description provided."}</p>
                           </div>
                           
                           {g.adminResponse && (
                             <div>
                               <p className="text-[10px] font-black text-blue-500 uppercase tracking-widest mb-2">Admin Response</p>
                               <p className="text-sm font-bold text-blue-900 leading-relaxed bg-blue-50 p-4 rounded-xl">{g.adminResponse}</p>
                             </div>
                           )}

                           {activeTab === 'global' && canManage && (
                             <div className="pt-6 border-t border-slate-100">
                               <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-4">Respond to Grievance</p>
                               <div className="space-y-4">
                                 <textarea 
                                   placeholder="Type response to member..."
                                   value={adminResponse} onChange={e => setAdminResponse(e.target.value)}
                                   className="w-full h-24 bg-slate-50 border border-slate-200 rounded-xl p-4 text-sm font-bold outline-none resize-none"
                                 />
                                 <div className="flex gap-4">
                                   <select 
                                     value={adminStatus} onChange={e => setAdminStatus(e.target.value)}
                                     className="bg-slate-50 border border-slate-200 rounded-xl px-4 text-sm font-bold outline-none"
                                   >
                                     <option value="IN_PROGRESS">Mark In Progress</option>
                                     <option value="RESOLVED">Mark Resolved</option>
                                     <option value="CLOSED">Close Ticket</option>
                                   </select>
                                   <Button onClick={() => handleRespond(g.id)} disabled={responding} className="flex-1 bg-slate-900 hover:bg-blue-600">
                                     {responding ? <Loader2 size={16} className="animate-spin" /> : <><Send size={14} className="mr-2" /> Send Response via WhatsApp</>}
                                   </Button>
                                 </div>
                               </div>
                             </div>
                           )}
                         </div>
                       </motion.div>
                     )}
                   </AnimatePresence>
                </motion.div>
              ))}
           </div>
        </div>

        {/* Intelligence Sidebar: New Report */}
        <div className="xl:col-span-1 space-y-10">
           <div className="glass-panel p-10 bg-slate-900 text-white overflow-hidden relative">
              <div className="absolute top-0 right-0 w-32 h-32 bg-blue-600/10 blur-3xl rounded-full" />
              <div className="relative z-10">
                 <h3 className="text-xl font-[900] tracking-tighter italic uppercase mb-8">Dispute Protocol</h3>
                 <div className="space-y-6">
                    {[
                      'Report with factual evidence only.',
                      'A Liaison will contact within 24 hours.',
                      'Escalate via Master Settings if needed.',
                      'Final resolution is digitally archived.'
                    ].map((step, i) => (
                      <div key={i} className="flex items-start gap-4">
                         <div className="w-1.5 h-1.5 rounded-full bg-blue-500 mt-2 shrink-0 shadow-[0_0_10px_rgba(59,130,246,0.5)]" />
                         <p className="text-[11px] font-bold text-slate-400 uppercase tracking-wider leading-relaxed">{step}</p>
                      </div>
                    ))}
                 </div>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
