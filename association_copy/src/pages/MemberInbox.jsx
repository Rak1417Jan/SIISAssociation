import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Bell, ShieldCheck, Zap, Mail, Calendar, Info, CheckCircle2 } from 'lucide-react';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';

export default function MemberInbox() {
  const [messages, setMessages] = useState([
    { id: 'TX-8842', subject: 'Strategic Hub Update', date: '2h ago', type: 'Update', body: 'The 2026 Institutional Reference Guide has been updated globally. Please review the new compliance metrics.', isRead: false },
    { id: 'TX-7710', subject: 'Annual Assembly Call', date: '1d ago', type: 'Event', body: 'Dear member, join us for the Annual General Assembly. Ensure your voting credentials are active.', isRead: false },
    { id: 'TX-6604', subject: 'Payment Overdue Notice', date: '4d ago', type: 'Alert', body: 'Urgent: Institutional account requires immediate dues synchronization to prevent tier downgrade.', isRead: false },
  ]);

  const handleMarkRead = (id) => {
    setMessages(messages.map(m => m.id === id ? { ...m, isRead: true } : m));
    toast.success('Communication marked as read');
  };

  return (
    <div className="space-y-10">
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic uppercase">Institutional Inbox</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Received broadcasts & alerts</p>
        </div>
      </div>

      <div className="glass-panel p-10">
        <div className="flex items-center justify-between mb-8">
           <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic">All Communications</h3>
           <Bell className="text-blue-600" size={18} />
        </div>
        
        <div className="space-y-6">
          {messages.map((msg, i) => (
            <motion.div 
              key={msg.id} 
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.1 }}
              className={`p-8 rounded-[32px] border flex flex-col md:flex-row items-start md:items-center justify-between group transition-all shadow-sm ${msg.isRead ? 'bg-white border-transparent opacity-70' : 'bg-slate-50/50 border-slate-100 hover:bg-white'}`}
            >
               <div className="flex items-start gap-6 mb-4 md:mb-0">
                  <div className={`w-12 h-12 rounded-2xl bg-white flex items-center justify-center transition-colors shadow-sm shrink-0 mt-1 ${msg.isRead ? 'text-slate-300' : 'text-slate-400 group-hover:text-blue-600'}`}>
                     {msg.type === 'Alert' ? <Zap size={22} className={msg.isRead ? 'text-slate-300' : 'text-amber-500'} /> : <Mail size={22} />}
                  </div>
                  <div>
                     <div className="flex items-center gap-3 mb-2">
                       <p className={`text-lg font-[900] tracking-tight italic uppercase ${msg.isRead ? 'text-slate-500' : 'text-slate-900'}`}>{msg.subject}</p>
                       <Badge status={msg.type === 'Alert' ? 'pending' : 'active'} label={msg.type} />
                     </div>
                     <p className={`text-xs font-bold leading-relaxed max-w-2xl ${msg.isRead ? 'text-slate-400' : 'text-slate-500'}`}>{msg.body}</p>
                     <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-4">ID: {msg.id} • Received {msg.date}</p>
                  </div>
               </div>
               <button 
                 onClick={() => handleMarkRead(msg.id)}
                 disabled={msg.isRead}
                 className={`hidden md:flex items-center gap-2 px-6 py-3 bg-white border border-slate-100 rounded-xl text-[10px] font-black uppercase tracking-widest italic transition-all ${msg.isRead ? 'text-green-500 border-green-100' : 'text-slate-400 hover:text-slate-900 group-hover:shadow-md'}`}
               >
                 <CheckCircle2 size={16} /> {msg.isRead ? 'Read' : 'Mark Read'}
               </button>
            </motion.div>
          ))}
        </div>
      </div>
    </div>
  );
}
