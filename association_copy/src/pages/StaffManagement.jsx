import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Users, UserPlus, Search, Filter, ShieldCheck, MoreVertical, Mail, Phone,
  Zap, MoreHorizontal, ArrowUpRight, ChevronRight, UserCheck, Key, Power, UserMinus
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getStaffList } from '../services/staffService';
import { getStaffActivity } from '../services/staffService';
import { deactivateStaff } from '../services/staffService';
import { reactivateStaff } from '../services/staffService';
import { resetStaffPassword } from '../services/staffService';
import { createStaff } from '../services/staffService';
import { assignRoleToStaff } from '../services/rolesService';
import PermissionGate from '../components/PermissionGate';

export default function StaffManagement() {
  const [page, setPage] = useState(1);
  const [roleFilter, setRoleFilter] = useState('');
  const [search, setSearch] = useState('');
  const limit = 10;
  
  const { execute: fetchStaff, data: staffData, loading } = useApi(getStaffList);
  const { execute: doDeactivate } = useApi(deactivateStaff);
  const { execute: doReactivate } = useApi(reactivateStaff);
  const { execute: doResetPassword } = useApi(resetStaffPassword);
  const { execute: doAssignRole } = useApi(assignRoleToStaff);
  
  const [activeMenu, setActiveMenu] = useState(null);
  
  // Modals state
  const [showAddModal, setShowAddModal] = useState(false);
  const [showActivityModal, setShowActivityModal] = useState(false);
  const [selectedStaff, setSelectedStaff] = useState(null);

  useEffect(() => {
    fetchStaff({ role: roleFilter }, page, limit);
  }, [page, roleFilter, fetchStaff]);

  const handleDeactivate = async (id) => {
    if (!window.confirm("Are you sure you want to deactivate this staff member?")) return;
    const tid = toast.loading('Deactivating...');
    const res = await doDeactivate(id);
    if (!res.error) {
       toast.success('Staff deactivated', { id: tid });
       fetchStaff({ role: roleFilter }, page, limit);
    } else {
       toast.error('Failed to deactivate', { id: tid });
    }
  };

  const handleReactivate = async (id) => {
    const tid = toast.loading('Reactivating...');
    const res = await doReactivate(id);
    if (!res.error) {
       toast.success('Staff reactivated', { id: tid });
       fetchStaff({ role: roleFilter }, page, limit);
    } else {
       toast.error('Failed to reactivate', { id: tid });
    }
  };

  const handleResetPassword = async (id) => {
    if (!window.confirm("Send password reset email to this staff member?")) return;
    const tid = toast.loading('Resetting password...');
    const res = await doResetPassword(id);
    if (!res.error) {
       toast.success(res.data.message || 'Password reset email sent', { id: tid });
    } else {
       toast.error('Failed to reset password', { id: tid });
    }
  };

  const handleAssignRole = async (id, newRole) => {
    if (!newRole) return;
    const tid = toast.loading('Changing role...');
    const res = await doAssignRole(id, newRole);
    if (!res.error) {
       toast.success('Role updated successfully', { id: tid });
       fetchStaff({ role: roleFilter }, page, limit);
    } else {
       toast.error('Failed to change role', { id: tid });
    }
  };

  const staff = staffData?.data?.data || [];
  const total = staffData?.data?.total || 0;
  const totalPages = Math.ceil(total / limit) || 1;

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">HUMAN CAPITAL</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Internal personnel access & oversight</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2">
            <UserCheck size={14} />
            Access Audit
          </Button>
          <PermissionGate action="manage" resource="staff">
            <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20" onClick={() => setShowAddModal(true)}>
              <UserPlus size={16} />
              Onboard Personnel
            </Button>
          </PermissionGate>
        </div>
      </div>

      {/* Roster IQ Bar */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
         {[
           { label: 'Active Personnel', value: '14 Active', drift: 'Stable', icon: Users, color: 'text-blue-500' },
           { label: 'Security Clearance', value: '100% Pass', drift: 'Verified', icon: ShieldCheck, color: 'text-green-500' },
           { label: 'Network Throughput', value: '840 Actions', drift: '+12%', icon: Zap, color: 'text-amber-500' },
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

      {/* Staff Roster Index */}
      <div className="glass-panel overflow-hidden border-slate-100">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
           <div className="relative max-w-sm w-full">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
              <input 
                type="text" 
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search staff identity or role..." 
                className="w-full pl-12 pr-6 py-4 bg-white border border-slate-100 rounded-[20px] text-xs font-bold outline-none focus:border-blue-500 transition-all shadow-sm"
              />
           </div>
           <div className="flex items-center gap-3">
              <select 
                value={roleFilter}
                onChange={(e) => { setRoleFilter(e.target.value); setPage(1); }}
                className="px-6 py-4 bg-white border border-slate-100 rounded-[20px] text-[10px] font-black text-slate-400 uppercase tracking-widest outline-none hover:text-slate-900 transition-all shadow-sm cursor-pointer"
              >
                 <option value="">All Clearances</option>
                 <option value="admin">Admin</option>
                 <option value="finance">Finance</option>
                 <option value="operator">Operator</option>
              </select>
           </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-slate-50/20">
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Personnel Identity</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Operational Role</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Security Index</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Live Status</th>
                <th className="px-8 py-6 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Intel</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {loading ? (
                <tr><td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">Loading...</td></tr>
              ) : staff.length === 0 ? (
                <tr><td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">No personnel found</td></tr>
              ) : staff.map((person, i) => (
                <motion.tr 
                  key={person.id}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.05 }}
                  className="group hover:bg-slate-50/50 transition-all"
                >
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-4">
                       <div className="w-10 h-10 rounded-xl bg-white border border-slate-100 flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors shadow-sm">
                          <Users size={18} />
                       </div>
                       <div className="flex flex-col">
                          <span className="text-sm font-[900] text-slate-900 tracking-tight italic group-hover:text-blue-600 transition-colors uppercase">{person.name}</span>
                          <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">{person.id}</span>
                       </div>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex flex-col">
                       <span className="text-xs font-black text-slate-800 tracking-tight uppercase mb-1">{person.role}</span>
                       <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{person.email}</span>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <span className="text-[10px] font-black text-blue-600 uppercase tracking-widest px-3 py-1.5 bg-blue-50 rounded-lg">{person.role} Clearance</span>
                  </td>
                  <td className="px-8 py-6 text-center">
                    <div className="flex items-center justify-center gap-2">
                       <div className={`w-2 h-2 rounded-full ${person.isActive ? 'bg-green-500 animate-pulse' : 'bg-slate-300'}`} />
                       <span className={`text-[9px] font-black uppercase tracking-widest ${person.isActive ? 'text-green-600' : 'text-slate-400'}`}>
                          {person.isActive ? 'Active' : 'Inactive'}
                       </span>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center justify-center gap-2 relative">
                       <button onClick={() => { setSelectedStaff(person); setShowActivityModal(true); }} className="p-3 bg-white border border-slate-100 rounded-xl text-slate-300 hover:text-blue-600 transition-all shadow-sm" title="View Activity">
                         <ArrowUpRight size={16} />
                       </button>
                       <PermissionGate action="manage" resource="staff">
                         <button onClick={() => activeMenu === person.id ? setActiveMenu(null) : setActiveMenu(person.id)} className="p-3 bg-white border border-slate-100 rounded-xl text-slate-300 hover:text-slate-900 transition-all shadow-sm" title="Manage Staff">
                           <MoreHorizontal size={16} />
                         </button>
                       </PermissionGate>
                       
                       {activeMenu === person.id && (
                         <div className="absolute right-0 top-12 w-48 bg-white border border-slate-100 rounded-xl shadow-xl z-50 py-2">
                            <div className="px-4 py-2 border-b border-slate-50">
                               <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-2">Change Role</p>
                               <select 
                                 className="w-full text-xs font-bold text-slate-700 bg-slate-50 border border-slate-100 rounded outline-none p-1"
                                 onChange={(e) => { handleAssignRole(person.id, e.target.value); setActiveMenu(null); }}
                                 value=""
                               >
                                  <option value="" disabled>Select role...</option>
                                  <option value="super_admin">Super Admin</option>
                                  <option value="admin">Admin</option>
                                  <option value="finance">Finance</option>
                                  <option value="operator">Operator</option>
                               </select>
                            </div>
                            <button onClick={() => { handleResetPassword(person.id); setActiveMenu(null); }} className="w-full text-left px-4 py-2 text-xs font-bold text-slate-700 hover:bg-slate-50 flex items-center gap-2">
                               <Key size={14} className="text-slate-400" /> Reset Password
                            </button>
                            {person.isActive ? (
                               <button onClick={() => { handleDeactivate(person.id); setActiveMenu(null); }} className="w-full text-left px-4 py-2 text-xs font-bold text-red-600 hover:bg-red-50 flex items-center gap-2">
                                  <UserMinus size={14} className="text-red-500" /> Deactivate
                               </button>
                            ) : (
                               <button onClick={() => { handleReactivate(person.id); setActiveMenu(null); }} className="w-full text-left px-4 py-2 text-xs font-bold text-green-600 hover:bg-green-50 flex items-center gap-2">
                                  <Power size={14} className="text-green-500" /> Reactivate
                               </button>
                            )}
                         </div>
                       )}
                    </div>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>
        
        <div className="p-10 bg-slate-50/30 border-t border-slate-100 flex items-center justify-between">
           <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Showing page {page} of {totalPages} • {total} Records</p>
           <div className="flex items-center gap-4">
              <button onClick={() => setPage(p => Math.max(1, p-1))} className="px-6 py-3 border border-slate-200 rounded-[14px] text-[10px] font-black text-slate-400 bg-white hover:bg-slate-50 transition-all uppercase tracking-widest">Previous</button>
              <button onClick={() => setPage(p => Math.min(totalPages, p+1))} className="px-6 py-3 border border-slate-200 rounded-[14px] text-[10px] font-black text-slate-900 bg-white hover:bg-slate-50 transition-all uppercase tracking-widest">Next</button>
           </div>
        </div>
      </div>
      
      {/* Activity Modal */}
      <AnimatePresence>
         {showActivityModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-slate-900/40 backdrop-blur-sm">
               <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0, scale: 0.95 }} className="bg-white rounded-3xl p-8 max-w-md w-full shadow-2xl">
                  <div className="flex justify-between items-center mb-6">
                     <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase">Activity Log</h3>
                     <button onClick={() => setShowActivityModal(false)} className="text-slate-400 hover:text-slate-900 text-sm font-black">CLOSE</button>
                  </div>
                  <ActivityViewer staffId={selectedStaff?.id} />
               </motion.div>
            </div>
         )}
      </AnimatePresence>

      {/* Add Staff Modal */}
      <AnimatePresence>
         {showAddModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-slate-900/40 backdrop-blur-sm">
               <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0, scale: 0.95 }} className="bg-white rounded-3xl p-8 max-w-md w-full shadow-2xl">
                  <div className="flex justify-between items-center mb-6">
                     <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase">Onboard Personnel</h3>
                     <button onClick={() => setShowAddModal(false)} className="text-slate-400 hover:text-slate-900 text-sm font-black">CLOSE</button>
                  </div>
                  <AddStaffForm onSuccess={() => { setShowAddModal(false); fetchStaff({ role: roleFilter }, page, limit); }} />
               </motion.div>
            </div>
         )}
      </AnimatePresence>
    </div>
  );
}

function ActivityViewer({ staffId }) {
  const { execute, data, loading } = useApi(getStaffActivity);
  useEffect(() => { if (staffId) execute(staffId); }, [staffId, execute]);
  
  if (loading) return <div className="p-4 text-center text-xs font-bold text-slate-400">Loading activity...</div>;
  const activities = data?.data || [];
  if (activities.length === 0) return <div className="p-4 text-center text-xs font-bold text-slate-400">No recent activity</div>;
  
  return (
    <div className="space-y-4 max-h-[400px] overflow-y-auto pr-2">
      {activities.map((act, i) => (
        <div key={i} className="flex gap-4 p-4 rounded-xl bg-slate-50 border border-slate-100">
           <div className="mt-1 w-2 h-2 rounded-full bg-blue-500 shrink-0" />
           <div>
             <p className="text-xs font-bold text-slate-900">{act.action} - <span className="text-blue-600">{act.target}</span></p>
             <p className="text-[10px] font-black text-slate-400 uppercase mt-1">{act.timestamp} • IP: {act.ip}</p>
           </div>
        </div>
      ))}
    </div>
  );
}

function AddStaffForm({ onSuccess }) {
  const [formData, setFormData] = useState({ name: '', email: '', phone: '', role: 'admin' });
  const { execute, loading } = useApi(createStaff);
  const [successData, setSuccessData] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const tid = toast.loading('Creating staff...');
    const res = await execute(formData);
    if (!res.error) {
      toast.success('Staff created successfully', { id: tid });
      setSuccessData(res.data.staff);
    } else {
      toast.error('Failed to create staff', { id: tid });
    }
  };

  if (successData) {
     return (
       <div className="text-center">
          <div className="w-16 h-16 bg-green-50 text-green-500 rounded-full flex items-center justify-center mx-auto mb-4"><UserCheck size={32} /></div>
          <h4 className="text-lg font-[900] text-slate-900 mb-2">Personnel Onboarded</h4>
          <p className="text-xs font-bold text-slate-500 mb-6">Staff account has been created. They must use the temporary credentials below to log in for the first time.</p>
          <div className="bg-slate-50 p-4 rounded-xl mb-6 text-left">
             <p className="text-[10px] font-black text-slate-400 uppercase">Email</p>
             <p className="text-sm font-bold text-slate-900 mb-3">{successData.email}</p>
             <p className="text-[10px] font-black text-slate-400 uppercase">Temporary Password</p>
             <p className="text-sm font-mono font-bold text-blue-600 tracking-wider">{successData.temporaryPassword}</p>
          </div>
          <Button onClick={onSuccess} className="w-full">Done</Button>
       </div>
     );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
         <label className="text-[10px] font-black text-slate-400 uppercase">Full Name</label>
         <input required type="text" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold focus:border-blue-500 outline-none" />
      </div>
      <div>
         <label className="text-[10px] font-black text-slate-400 uppercase">Email Address</label>
         <input required type="email" value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold focus:border-blue-500 outline-none" />
      </div>
      <div>
         <label className="text-[10px] font-black text-slate-400 uppercase">Phone Number</label>
         <input required type="text" value={formData.phone} onChange={e => setFormData({...formData, phone: e.target.value})} className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold focus:border-blue-500 outline-none" />
      </div>
      <div>
         <label className="text-[10px] font-black text-slate-400 uppercase">Initial Role</label>
         <select value={formData.role} onChange={e => setFormData({...formData, role: e.target.value})} className="w-full px-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold focus:border-blue-500 outline-none">
            <option value="admin">Admin</option>
            <option value="finance">Finance</option>
            <option value="operator">Operator</option>
         </select>
      </div>
      <Button type="submit" disabled={loading} className="w-full mt-4">
        {loading ? 'Processing...' : 'Create Account'}
      </Button>
    </form>
  );
}
