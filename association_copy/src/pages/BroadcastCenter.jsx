import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Send, 
  Users, 
  Smartphone, 
  Mail, 
  Bell, 
  History, 
  ArrowRight,
  ChevronRight,
  ShieldCheck,
  Zap,
  Globe,
  Plus,
  FileText,
  Clock,
  CheckCircle2
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getBroadcasts, createBroadcast, sendBroadcast, scheduleBroadcast, cancelBroadcast, getBroadcastStats } from '../services/broadcastService';

export default function BroadcastCenter() {
  const [channel, setChannel] = useState('WHATSAPP');
  const [recipientFilter, setRecipientFilter] = useState('ALL');
  const [isHistoryOpen, setIsHistoryOpen] = useState(false);
  const [sending, setSending] = useState(false);
  const [message, setMessage] = useState('');
  
  const { execute: fetchBroadcasts, data: broadcastsData, loading: broadcastsLoading } = useApi(getBroadcasts);
  const { execute: doCreate } = useApi(createBroadcast);
  const { execute: doSend } = useApi(sendBroadcast);
  const { execute: doCancel } = useApi(cancelBroadcast);

  React.useEffect(() => {
    if (isHistoryOpen) fetchBroadcasts();
  }, [isHistoryOpen, fetchBroadcasts]);

  const handleBroadcast = async () => {
    if (!message) {
      toast.error('Broadcasting an empty payload is prohibited');
      return;
    }
    setSending(true);
    const tid = toast.loading(`Synchronizing ${channel} transmission clusters...`);
    const res = await doCreate({ title: 'System Notice', message, channel, recipientFilter });
    if (!res.error) {
       await doSend(res.data.broadcast.id);
       toast.success('Broadcast successfully propagated globally', { id: tid });
       setMessage('');
    } else {
       toast.error('Terminal transmission failure', { id: tid });
    }
    setSending(false);
  };

  const broadcastsList = broadcastsData?.data?.data || [];

  const templates = [
    { title: 'Annual Meeting Invite', body: 'Dear {name}, join us for the Annual General Assembly on {date}.' },
    { title: 'Payment Overdue Notice', body: 'Urgent: Institutional account {id} requires immediate dues synchronization.' },
    { title: 'Strategic Policy Update', body: 'The 2026 Institutional Reference Guide has been updated globally.' }
  ];

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic uppercase">Comms Hub</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional broadcast & member outreach</p>
        </div>
        <button 
          onClick={() => setIsHistoryOpen(true)}
          className="flex items-center gap-3 px-8 py-4 bg-slate-50 border border-slate-100 rounded-2xl text-[10px] font-black text-slate-400 hover:text-slate-900 transition-all uppercase tracking-widest italic group shadow-sm active:scale-95"
        >
          <History size={16} className="group-hover:rotate-[-20deg] transition-all" />
          Transmission History
        </button>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-12">
         {/* Broadcast Composer */}
         <div className="xl:col-span-2 glass-panel p-10 lg:p-14 relative overflow-hidden">
            <div className="absolute top-0 right-0 w-80 h-80 bg-blue-500/5 blur-[120px] rounded-full pointer-events-none" />
            
            <div className="mb-12 relative z-10">
               <h3 className="text-2xl font-[900] text-slate-900 tracking-tight italic uppercase">Message Composer</h3>
               <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1 italic">Initiate global or targeted operational transmission</p>
            </div>

            <div className="space-y-12 relative z-10">
               {/* Channel Selector */}
                <div className="space-y-5">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Transmission Channel</p>
                  <div className="flex bg-slate-50/80 p-2 rounded-[32px] max-w-lg border border-slate-100">
                    {[
                      { id: 'WHATSAPP', icon: Smartphone, label: 'WhatsApp' },
                      { id: 'SMS', icon: Mail, label: 'SMS Cell' },
                      { id: 'BOTH', icon: Bell, label: 'Dual Cast' },
                    ].map(c => (
                      <button
                        key={c.id}
                        onClick={() => setChannel(c.id)}
                        className={`flex-1 py-4 px-2 flex items-center justify-center gap-3 rounded-[24px] transition-all duration-500
                          ${channel === c.id ? 'bg-white text-blue-600 shadow-2xl shadow-blue-500/5 ring-1 ring-slate-100' : 'text-slate-400 hover:text-slate-600'}
                        `}
                      >
                        <c.icon size={18} />
                        <span className="text-[10px] font-black uppercase tracking-widest">{c.label}</span>
                      </button>
                    ))}
                  </div>
               </div>

               {/* Recipient Targeting */}
               <div className="space-y-5">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Targeting Segments</p>
                  <div className="flex flex-wrap gap-3">
                     {[
                       { id: 'ALL', label: 'All Members' },
                       { id: 'APPROVED', label: 'Approved Only' },
                       { id: 'EXPIRING_SOON', label: 'Expiring Soon' }
                     ].map((tag, i) => (
                       <button key={i} onClick={() => { setRecipientFilter(tag.id); toast.success(`Targeting updated: ${tag.label}`, {icon: '🎯'}); }} className={`px-6 py-3.5 rounded-[18px] text-[10px] font-black uppercase tracking-widest border-2 transition-all duration-300 active:scale-95
                         ${recipientFilter === tag.id ? 'bg-blue-600 border-blue-600 text-white shadow-xl shadow-blue-500/20' : 'bg-white border-slate-50 text-slate-400 hover:border-blue-100 hover:text-slate-900'}
                       `}>
                         {tag.label}
                       </button>
                     ))}
                     <button onClick={() => toast('Opening Segment Builder...')} className="w-12 h-12 rounded-[18px] border-2 border-dashed border-slate-200 flex items-center justify-center text-slate-300 hover:border-blue-400 hover:text-blue-500 transition-all active:scale-95">
                        <Plus size={18} />
                     </button>
                  </div>
               </div>

               {/* Message Body */}
               <div className="space-y-5">
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">Transmission Payload</p>
                  <div className="relative">
                     <textarea 
                        rows="6"
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                        placeholder={`Type the ${channel} payload structure... (Variables: {name}, {id}, {company})`}
                        className="w-full bg-slate-50/50 border border-slate-100 rounded-[40px] p-10 text-sm font-bold text-slate-800 outline-none focus:border-blue-500 focus:bg-white transition-all shadow-inner"
                     />
                     <div className="absolute bottom-10 right-10 flex items-center gap-3">
                        <div className="text-[9px] font-black text-slate-300 uppercase tracking-widest">
                           {message.length} Characters
                        </div>
                        <div className="w-[1px] h-3 bg-slate-200" />
                        <div className="text-[9px] font-black text-blue-500 uppercase tracking-widest italic">
                           4.2K Units Left
                        </div>
                     </div>
                  </div>
               </div>

               <div className="pt-10 flex justify-end items-center gap-6">
                  <button 
                    onClick={() => setMessage('')}
                    className="text-[10px] font-black text-slate-400 hover:text-red-500 uppercase tracking-widest transition-colors active:scale-95"
                  >
                    Purge Draft
                  </button>
                  <Button 
                    onClick={handleBroadcast}
                    disabled={sending}
                    className="py-6 px-14 rounded-[28px] bg-blue-600 hover:bg-blue-700 text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/20 flex items-center gap-4 group transition-all h-auto active:scale-95"
                  >
                    {sending ? <span className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : <>
                      Launch Blast
                      <Send size={18} className="group-hover:translate-x-1 group-hover:-translate-y-1 group-hover:rotate-12 transition-all duration-500" />
                    </>}
                  </Button>
               </div>
            </div>
         </div>

         {/* Sidebar: Outreach Stats */}
         <div className="xl:col-span-1 space-y-12">
            <div className="glass-panel p-12 bg-slate-900 text-white overflow-hidden relative min-h-[380px]">
               <div className="absolute top-[-20%] right-[-20%] w-64 h-64 bg-blue-600/20 blur-[120px] rounded-full pointer-events-none" />
               <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-10">Outreach Intelligence</p>
               <div className="space-y-10 relative z-10">
                  {[
                    { label: 'Network Penetration', value: '84.2%', icon: Zap, color: 'text-amber-500' },
                    { label: 'Propagation Reach', value: '4.2K Nodes', icon: Globe, color: 'text-blue-500' },
                    { label: 'Response Integrity', value: 'Critical', icon: ShieldCheck, color: 'text-green-500' },
                  ].map((s, i) => (
                    <div key={i} className="flex items-center gap-6 group cursor-pointer">
                       <div className={`w-14 h-14 rounded-[24px] bg-white/5 border border-white/10 flex items-center justify-center ${s.color} group-hover:bg-white/10 transition-colors shadow-inner`}>
                          <s.icon size={24} />
                       </div>
                       <div>
                          <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">{s.label}</p>
                          <p className="text-2xl font-[900] tracking-tighter italic uppercase">{s.value}</p>
                       </div>
                    </div>
                  ))}
               </div>
            </div>

            <div className="glass-panel p-10 relative overflow-hidden">
               <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 blur-3xl rounded-full" />
               <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-8 italic uppercase relative z-10">Institutional Templates</h3>
               <div className="space-y-4 relative z-10">
                  {templates.map((t, i) => (
                    <button 
                      key={i} 
                      onClick={() => setMessage(t.body)}
                      className="w-full text-left p-6 bg-slate-50/50 rounded-3xl border border-slate-100 hover:bg-white hover:shadow-2xl hover:border-transparent transition-all group active:scale-[0.98]"
                    >
                       <div className="flex items-center justify-between">
                          <span className="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover:text-slate-900 transition-colors">{t.title}</span>
                          <div className="w-8 h-8 rounded-lg bg-white border border-slate-50 flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-all opacity-0 group-hover:opacity-100">
                             <ChevronRight size={16} />
                          </div>
                       </div>
                    </button>
                  ))}
               </div>
            </div>
         </div>
      </div>

      {/* History Modal */}
      <Modal
        isOpen={isHistoryOpen}
        onClose={() => setIsHistoryOpen(false)}
        title="Institutional Transmission History"
      >
         <div className="space-y-6 max-h-[500px] overflow-y-auto pr-4 scrollbar-hide">
            {broadcastsLoading ? (
               <div className="text-center text-xs font-bold text-slate-400 py-10">Fetching transmission history...</div>
            ) : broadcastsList.map((tx, i) => (
               <div key={tx.id} className="p-8 bg-slate-50 rounded-[32px] border border-slate-100 flex items-center justify-between group hover:bg-white hover:shadow-xl transition-all">
                  <div className="flex items-center gap-6">
                     <div className={`w-12 h-12 rounded-2xl flex items-center justify-center shadow-inner
                        ${tx.status === 'SENT' ? 'bg-green-50 text-green-600' : tx.status === 'SCHEDULED' ? 'bg-blue-50 text-blue-600' : 'bg-amber-50 text-amber-600'}
                     `}>
                        {tx.status === 'SENT' ? <CheckCircle2 size={22} /> : tx.status === 'SCHEDULED' ? <Clock size={22} /> : <Zap size={22} />}
                     </div>
                     <div>
                        <p className="text-sm font-[900] text-slate-900 tracking-tight italic uppercase">{tx.title}</p>
                        <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">ID: {tx.id} • {tx.sentAt || tx.scheduledAt || 'Draft'}</p>
                     </div>
                  </div>
                  <div className="flex items-center gap-4">
                     <div className="text-right">
                        <p className="text-xs font-black text-slate-900 italic tracking-tighter uppercase">{tx.recipientCount} Nodes</p>
                        <p className="text-[9px] font-black text-slate-300 uppercase tracking-widest mt-1">{tx.status}</p>
                     </div>
                     <div className="w-[1px] h-8 bg-slate-200 mx-2" />
                     {tx.status === 'DRAFT' && (
                        <button onClick={async () => {
                           const res = await doSend(tx.id);
                           if (!res.error) toast.success('Sent!');
                           fetchBroadcasts();
                        }} className="p-2 text-blue-600 hover:bg-blue-50 rounded-xl transition-colors">
                           <Send size={16} />
                        </button>
                     )}
                     {tx.status === 'SCHEDULED' && (
                        <button onClick={async () => {
                           const res = await doCancel(tx.id);
                           if (!res.error) toast.success('Cancelled!');
                           fetchBroadcasts();
                        }} className="text-[10px] font-black text-red-500 hover:bg-red-50 px-3 py-2 rounded-xl transition-colors uppercase">
                           Cancel
                        </button>
                     )}
                  </div>
               </div>
            ))}
         </div>
         <div className="pt-8 mt-4 border-t border-slate-100 flex justify-end">
            <button 
              onClick={() => setIsHistoryOpen(false)}
              className="px-8 py-4 rounded-xl bg-slate-900 text-white text-[10px] font-black uppercase tracking-widest hover:bg-blue-600 transition-colors shadow-2xl"
            >
              Close Ledger
            </button>
         </div>
      </Modal>
    </div>
  );
}
