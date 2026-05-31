import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Search, 
  LifeBuoy, 
  MessageCircle, 
  BookOpen, 
  ShieldCheck, 
  ArrowRight,
  ChevronRight,
  Zap,
  Star,
  Globe
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { searchMembers } from '../services/memberService';

export default function SupportLookup() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [results, setResults] = useState(null);
  const { execute: doSearch, loading } = useApi(searchMembers);

  useEffect(() => {
    if (!search) {
      setResults(null);
      return;
    }
    const timer = setTimeout(async () => {
      const res = await doSearch(search);
      if (res.data) setResults(res.data);
    }, 300);
    return () => clearTimeout(timer);
  }, [search, doSearch]);

  const [selectedMember, setSelectedMember] = useState(null);
  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">SUPPORT CONCIERGE</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional help desk & resource mapping</p>
        </div>
      </div>

      {/* Primary Search Unit */}
      <div className="glass-panel p-10 lg:p-20 bg-slate-900 overflow-hidden relative">
         <div className="absolute top-0 right-0 w-96 h-96 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
         <div className="relative z-10 text-center max-w-2xl mx-auto">
            <h2 className="text-3xl font-[900] text-white tracking-tighter mb-8 uppercase italic italic">How can we assist your operation?</h2>
            <div className="relative group">
               <Search className="absolute left-6 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-blue-500 transition-colors" size={24} />
               <input 
                  type="text" 
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Ask a question or search for a member by name, ID or phone..." 
                  className="w-full pl-16 pr-8 py-6 bg-white/5 border border-white/10 rounded-[32px] text-white font-bold outline-none focus:bg-white focus:text-slate-900 transition-all transition-duration-500 shadow-2xl"
               />
               
               {/* Search Results Dropdown/Area */}
               {search && (
                 <div className="absolute top-full left-0 right-0 mt-4 bg-white rounded-[32px] shadow-2xl p-6 z-50 text-left max-h-[400px] overflow-y-auto">
                    {loading ? (
                       <div className="p-8 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">Searching...</div>
                    ) : results && results.length > 0 ? (
                       <div className="space-y-3">
                          {results.map(m => (
                            <div key={m.id} onClick={() => setSelectedMember(m)} className="p-4 bg-slate-50 border border-slate-100 rounded-2xl hover:border-blue-500/50 cursor-pointer transition-all flex items-center justify-between group">
                               <div>
                                  <h4 className="text-sm font-[900] text-slate-900 tracking-tight uppercase group-hover:text-blue-600 transition-colors">{m.name}</h4>
                                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{m.id} • {m.phone}</p>
                               </div>
                               <Badge status={m.status.toLowerCase()} label={m.status} />
                            </div>
                          ))}
                       </div>
                    ) : (
                       <div className="p-8 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">No results found</div>
                    )}
                 </div>
               )}
            </div>
            <div className="mt-8 flex flex-wrap justify-center gap-3">
               {['Membership Billing', 'ID Card Dispatch', 'Event Registration', 'Grievance Tracking'].map((tag, i) => (
                 <span key={i} onClick={() => toast(`Filtering by: ${tag}`, { icon: '🔍' })} className="px-4 py-2 bg-white/5 border border-white/5 rounded-full text-[9px] font-black uppercase tracking-widest text-slate-400 hover:text-white hover:border-white/20 cursor-pointer transition-all active:scale-95">
                   {tag}
                 </span>
               ))}
            </div>
         </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
         {/* Support Channels */}
         <div className="xl:col-span-2 grid grid-cols-1 sm:grid-cols-2 gap-8">
            {[
              { icon: MessageCircle, title: 'Institutional Chat', text: 'Real-time synchronization with a support liaison.', color: 'text-blue-500 bg-blue-50' },
              { icon: Globe, title: 'Knowledge Base', text: 'Deep technical documentation and policy guides.', color: 'text-purple-500 bg-purple-50' },
              { icon: ShieldCheck, title: 'Security Desk', text: 'Escalate credential or access-level grievances.', color: 'text-green-500 bg-green-50' },
              { icon: BookOpen, title: 'Member Handbook', text: 'Official 2026 institutional reference guide.', color: 'text-amber-500 bg-amber-50' },
            ].map((channel, i) => (
              <motion.div 
                key={i}
                initial={{ opacity: 0, y: 15 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.1 }}
                className="glass-panel p-8 group hover:shadow-xl transition-all cursor-pointer"
              >
                 <div className={`w-12 h-12 rounded-2xl ${channel.color} flex items-center justify-center mb-6 shadow-inner`}>
                    <channel.icon size={22} />
                 </div>
                 <h3 className="text-lg font-[900] text-slate-900 tracking-tight uppercase italic mb-2">{channel.title}</h3>
                 <p className="text-[11px] font-bold text-slate-400 leading-relaxed uppercase tracking-wider mb-6">{channel.text}</p>
                 <button onClick={() => toast.success(`${channel.title} initialized`)} className="text-[10px] font-black text-blue-600 uppercase tracking-widest flex items-center gap-2 group-hover:translate-x-1 transition-transform">
                   Initiate <ArrowRight size={14} />
                 </button>
              </motion.div>
            ))}
         </div>

         {/* Intelligence Sidebar: FAQ Flow */}
         <div className="xl:col-span-1 glass-panel p-10 bg-slate-50/50">
            <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-8">Priority FAQ</h3>
            <div className="space-y-4">
               {[
                 'How do I regenerate my digital ID?',
                 'Renewal cycle for Elite Tier members?',
                 'Updating institutional GST details?',
                 'Dispatch timeline for physical kits?'
               ].map((q, i) => (
                 <div key={i} onClick={() => toast(`Loading response for: ${q}`)} className="p-6 bg-white border border-slate-100 rounded-2xl hover:border-blue-500/50 group transition-all cursor-pointer active:scale-[0.98]">
                    <div className="flex items-center justify-between">
                       <span className="text-[10px] font-black text-slate-400 group-hover:text-slate-900 uppercase tracking-widest transition-colors leading-relaxed pr-4">{q}</span>
                       <ChevronRight size={14} className="text-slate-200 group-hover:text-blue-600 transition-all" />
                    </div>
                 </div>
               ))}
            </div>
            <div className="mt-12 p-8 bg-blue-600 rounded-[32px] text-white overflow-hidden relative">
               <div className="absolute top-0 right-0 w-24 h-24 bg-white/10 blur-2xl rounded-full" />
               <Zap className="text-blue-200 mb-4" size={24} />
               <p className="text-sm font-[900] italic leading-tight uppercase">Still unable to resolve?</p>
               <p className="text-[10px] font-bold text-blue-100 uppercase tracking-widest mt-2">Our executive desk is standing by.</p>
               <Button 
                  onClick={() => navigate('/grievance')}
                  className="w-full mt-6 bg-white text-blue-600 hover:bg-slate-50 font-black text-[10px] uppercase tracking-widest rounded-xl py-4"
                >
                  Raise Ticket
                </Button>
            </div>
         </div>
      </div>

       {/* Quick Detail Modal */}
       {selectedMember && (
         <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={() => setSelectedMember(null)} />
            <motion.div 
               initial={{ opacity: 0, scale: 0.95, y: 20 }}
               animate={{ opacity: 1, scale: 1, y: 0 }}
               className="relative bg-white rounded-[40px] w-full max-w-lg overflow-hidden shadow-2xl"
            >
               <div className="p-10 border-b border-slate-100">
                  <div className="flex items-center justify-between mb-6">
                     <Badge status={selectedMember.status.toLowerCase()} label={selectedMember.status} />
                     <button onClick={() => setSelectedMember(null)} className="text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-widest">Close</button>
                  </div>
                  <h2 className="text-3xl font-[900] text-slate-900 tracking-tighter italic uppercase">{selectedMember.name}</h2>
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{selectedMember.id}</p>
               </div>
               <div className="p-10 bg-slate-50 space-y-6">
                  <div>
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Affiliated Firm</p>
                     <p className="text-sm font-[900] text-slate-800 tracking-tight uppercase">{selectedMember.firmName}</p>
                  </div>
                  <div>
                     <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Contact</p>
                     <p className="text-sm font-[900] text-slate-800 tracking-tight uppercase">{selectedMember.email}</p>
                     <p className="text-sm font-[900] text-slate-800 tracking-tight uppercase">{selectedMember.phone}</p>
                  </div>
                  <Button 
                    className="w-full mt-4 bg-blue-600 hover:bg-blue-700 text-white font-black text-xs uppercase tracking-widest py-5 rounded-2xl flex justify-center"
                    onClick={() => navigate(`/member/${selectedMember.id}`)}
                  >
                     Open Full Dossier
                  </Button>
               </div>
            </motion.div>
         </div>
       )}
    </div>
  );
}
