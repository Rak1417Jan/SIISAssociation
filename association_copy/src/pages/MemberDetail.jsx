import React, { useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  User, 
  Building2, 
  Mail, 
  Phone, 
  MapPin, 
  Calendar, 
  ShieldCheck, 
  ArrowLeft, 
  Edit3,
  Trash2,
  FileText,
  BadgeCheck,
  History,
  Zap,
  MoreHorizontal
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMemberById, approveMember, rejectMember, suspendMember, reactivateMember, getMembershipHistory } from '../services/memberService';
import PermissionGate from '../components/PermissionGate';

export default function MemberDetail() {
  const navigate = useNavigate();
  const { id } = useParams();

  const { execute: fetchMember, data: response, loading } = useApi(getMemberById);
  const { execute: doApprove } = useApi(approveMember);
  const { execute: doReject } = useApi(rejectMember);
  const { execute: doSuspend } = useApi(suspendMember);
  const { execute: doReactivate } = useApi(reactivateMember);
  const { execute: fetchHistory, data: historyRes } = useApi(getMembershipHistory);

  useEffect(() => {
    fetchMember(id);
    fetchHistory(id);
    // eslint-disable-next-line
  }, [id]);

  const memberData = response?.data;
  
  if (loading) {
    return <div className="min-h-[50vh] flex items-center justify-center text-xs font-bold uppercase tracking-widest text-slate-400">Loading Intelligence...</div>;
  }

  if (!memberData) {
    return <div className="min-h-[50vh] flex items-center justify-center text-xs font-bold uppercase tracking-widest text-red-400">Dossier Not Found</div>;
  }

  const member = {
    name: memberData.name,
    id: memberData.id,
    role: memberData.planType + ' Member',
    tier: memberData.planType,
    status: memberData.status,
    loyaltyDX: '1,200',
    trustScore: '92.1',
    fields: [
      { icon: Building2, label: 'Affiliated Firm', value: memberData.firmName },
      { icon: Mail, label: 'Official Correspondence', value: memberData.email },
      { icon: Phone, label: 'Emergency Contact', value: memberData.phone },
      { icon: MapPin, label: 'Primary Node', value: memberData.address },
      { icon: Calendar, label: 'Activation Epoch', value: memberData.appliedAt },
    ],
    docs: memberData.documents.map(d => ({
      name: d.type,
      size: 'V-File',
      date: d.status
    }))
  };

  return (
    <div className="space-y-10">
      {/* Navigation Header */}
      <div className="flex items-center justify-between pb-6 border-b border-slate-100">
        <button onClick={() => navigate('/members')} className="flex items-center gap-3 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-[0.2em] transition-colors group">
          <div className="w-10 h-10 bg-slate-50 rounded-xl flex items-center justify-center group-hover:bg-white group-hover:shadow-lg transition-all">
            <ArrowLeft size={16} />
          </div>
          Back to Directory
        </button>
        <div className="flex items-center gap-3">
          <PermissionGate action="delete" resource="members">
            <Button variant="secondary" size="sm" className="bg-red-50 text-red-600 border-red-100 hover:bg-red-100 active:scale-95" onClick={() => toast.error('Account termination requires Level 5 Auth')}>
               <Trash2 size={14} />
            </Button>
          </PermissionGate>
          <Button size="sm" className="gap-2 active:scale-95" onClick={() => toast('Entering edit mode...', {icon: '✏️'})}>
            <Edit3 size={14} />
            Modify Dossier
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
        {/* Left Col: Primary Identity */}
        <div className="xl:col-span-1 space-y-8">
           <div className="glass-panel p-10 text-center relative overflow-hidden group">
              <div className="absolute top-0 left-0 w-full h-32 bg-slate-900" />
              <div className="relative pt-12">
                 <div className="relative inline-block">
                    <div className="w-40 h-40 rounded-[48px] bg-white p-2 shadow-2xl relative z-10 mx-auto overflow-hidden">
                       <div className="w-full h-full rounded-[40px] bg-slate-50 flex items-center justify-center text-slate-300">
                          <User size={64} />
                       </div>
                    </div>
                    <div className="absolute -bottom-2 -right-2 w-12 h-12 bg-blue-600 rounded-3xl flex items-center justify-center text-white border-4 border-white shadow-xl z-20">
                       <ShieldCheck size={20} />
                    </div>
                 </div>
                 
                 <h2 className="text-3xl font-[900] text-slate-900 mt-8 tracking-tighter italic uppercase">{member.name}</h2>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1 italic">{member.id}</p>
                 
                 <div className="flex items-center justify-center gap-3 mt-6">
                    <Badge status="active" label={`${member.tier} Tier`} />
                    <Badge status={member.status.toLowerCase()} label={member.status} />
                 </div>
              </div>

              <div className="mt-12 pt-10 border-t border-slate-50 grid grid-cols-2 gap-4">
                 <div className="text-center">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Loyalty DX</p>
                    <p className="text-xl font-[900] text-blue-600 tracking-tighter italic">{member.loyaltyDX}</p>
                 </div>
                 <div className="text-center">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Trust Score</p>
                    <p className="text-xl font-[900] text-green-500 tracking-tighter italic">{member.trustScore}</p>
                 </div>
              </div>
           </div>

           <div className="glass-panel p-10">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-8 italic">Quick Operations</h3>
              <div className="space-y-4">
                 <PermissionGate action="approve" resource="members">
                   <button onClick={async () => {
                      const tid = toast.loading('Approving...');
                      const res = await doApprove(id, 'Approved by admin');
                      if (!res.error) { toast.success('Approved successfully', {id: tid}); fetchMember(id); }
                      else toast.error('Failed to approve', {id: tid});
                   }} className="w-full p-5 bg-white border border-slate-50 rounded-2xl flex items-center justify-between group hover:shadow-xl hover:border-transparent transition-all active:scale-[0.98]">
                      <div className="flex items-center gap-4">
                         <div className="w-10 h-10 rounded-xl flex items-center justify-center text-green-500 bg-green-50 shadow-inner">
                            <ShieldCheck size={18} />
                         </div>
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors text-left">Approve Application</span>
                      </div>
                   </button>
                 </PermissionGate>
                 <PermissionGate action="reject" resource="members">
                   <button onClick={async () => {
                      const tid = toast.loading('Rejecting...');
                      const res = await doReject(id, 'Missing details');
                      if (!res.error) { toast.success('Rejected successfully', {id: tid}); fetchMember(id); }
                      else toast.error('Failed to reject', {id: tid});
                   }} className="w-full p-5 bg-white border border-slate-50 rounded-2xl flex items-center justify-between group hover:shadow-xl hover:border-transparent transition-all active:scale-[0.98]">
                      <div className="flex items-center gap-4">
                         <div className="w-10 h-10 rounded-xl flex items-center justify-center text-amber-500 bg-amber-50 shadow-inner">
                            <FileText size={18} />
                         </div>
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors text-left">Reject / Request Fix</span>
                      </div>
                   </button>
                 </PermissionGate>
                 <PermissionGate action="delete" resource="members">
                   <button onClick={async () => {
                      const tid = toast.loading('Suspending...');
                      const res = await doSuspend(id, 'Policy violation');
                      if (!res.error) { toast.success('Account suspended', {id: tid}); fetchMember(id); }
                      else toast.error('Failed to suspend', {id: tid});
                   }} className="w-full p-5 bg-white border border-slate-50 rounded-2xl flex items-center justify-between group hover:shadow-xl hover:border-transparent transition-all active:scale-[0.98]">
                      <div className="flex items-center gap-4">
                         <div className="w-10 h-10 rounded-xl flex items-center justify-center text-red-500 bg-red-50 shadow-inner">
                            <Zap size={18} />
                         </div>
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors text-left">Freeze Institutional Access</span>
                      </div>
                   </button>
                 </PermissionGate>
                 <button onClick={async () => {
                    const tid = toast.loading('Reactivating...');
                    const res = await doReactivate(id);
                    if (!res.error) { toast.success('Account reactivated', {id: tid}); fetchMember(id); }
                    else toast.error('Failed to reactivate', {id: tid});
                 }} className="w-full p-5 bg-white border border-slate-50 rounded-2xl flex items-center justify-between group hover:shadow-xl hover:border-transparent transition-all active:scale-[0.98]">
                    <div className="flex items-center gap-4">
                       <div className="w-10 h-10 rounded-xl flex items-center justify-center text-blue-500 bg-blue-50 shadow-inner">
                          <History size={18} />
                       </div>
                       <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors text-left">Reactivate Member</span>
                    </div>
                 </button>
              </div>
           </div>
        </div>

        {/* Right Col: Deep Intel */}
        <div className="xl:col-span-2 space-y-8">
           <div className="glass-panel p-10 lg:p-14">
              <div className="mb-12">
                 <h3 className="text-2xl font-[900] text-slate-900 tracking-tight italic uppercase">Institutional Dossier</h3>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Full intelligence report and data set</p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-x-16 gap-y-12">
                 {member.fields.map((field, i) => (
                    <div key={i} className="relative group">
                       <div className="absolute left-[-1.5rem] top-0 bottom-0 w-[2px] bg-slate-50 group-hover:bg-blue-600 transition-colors" />
                       <div className="flex items-center gap-3 mb-2">
                          <field.icon size={16} className="text-slate-300" />
                          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">{field.label}</p>
                       </div>
                       <p className="text-base font-[900] text-slate-800 tracking-tight uppercase italic">{field.value}</p>
                    </div>
                 ))}
              </div>

              <div className="mt-20 pt-10 border-t border-slate-50">
                 <div className="flex items-center justify-between mb-8">
                    <h4 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">Document Vault</h4>
                    <span className="text-[10px] font-black text-blue-600 uppercase tracking-widest bg-blue-50 px-3 py-1.5 rounded-lg">{member.docs.length} Secure Artifacts</span>
                 </div>
                 <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    {member.docs.map((doc, i) => (
                      <div onClick={() => toast(`Opening artifact: ${doc.name}`, {icon: '📄'})} key={i} className="p-5 bg-slate-50/50 rounded-2xl border border-slate-100 flex items-center justify-between group cursor-pointer hover:bg-white hover:shadow-xl hover:border-transparent transition-all active:scale-[0.98]">
                         <div className="flex items-center gap-4">
                            <div className="w-10 h-10 rounded-xl bg-white flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors">
                               <FileText size={18} />
                            </div>
                            <div>
                               <p className="text-[10px] font-black text-slate-900 uppercase tracking-wider mb-1 truncate max-w-[120px]">{doc.name}</p>
                               <p className="text-[9px] font-bold text-slate-400 uppercase tracking-widest">{doc.size} • {doc.date}</p>
                            </div>
                         </div>
                         <History size={14} className="text-slate-200 group-hover:text-blue-600 transition-colors" />
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
