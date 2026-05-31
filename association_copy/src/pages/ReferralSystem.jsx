import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Zap, 
  Gift, 
  Users, 
  ArrowRight, 
  Copy, 
  CheckCircle, 
  Award,
  Globe,
  TrendingUp,
  Share2,
  MoreVertical,
  ChevronRight,
  Send,
  Loader2
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getMemberReferrals, getReferralLeaderboard, sendReferralInvite, shareReferralLink } from '../services/engagementService';

export default function ReferralSystem() {
  const [inviteData, setInviteData] = useState({ name: '', phone: '', firmName: '', message: '' });
  
  const { execute: fetchReferrals, data: referralsData, loading: loadingReferrals } = useApi(getMemberReferrals);
  const { execute: fetchLeaderboard, data: leaderboardData } = useApi(getReferralLeaderboard);
  const { execute: doSendInvite, loading: sendingInvite } = useApi(sendReferralInvite);
  const { execute: doShare } = useApi(shareReferralLink);

  useEffect(() => {
    fetchReferrals('CURRENT_USER');
    fetchLeaderboard();
  }, [fetchReferrals, fetchLeaderboard]);

  const handleShare = async (channel) => {
    const tid = toast.loading('Processing share link...');
    const res = await doShare('CURRENT_USER', channel);
    if (!res.error) {
      if (channel === 'copy') {
        navigator.clipboard.writeText(res.data.referralLink);
      }
      toast.success(res.data.message, { id: tid });
    } else {
      toast.error('Failed to share link', { id: tid });
    }
  };

  const handleInviteSubmit = async (e) => {
    e.preventDefault();
    if (!inviteData.name || !inviteData.phone) {
      toast.error('Name and Phone are required');
      return;
    }
    const res = await doSendInvite('CURRENT_USER', inviteData);
    if (!res.error) {
      toast.success(res.data.message);
      setInviteData({ name: '', phone: '', firmName: '', message: '' });
    }
  };

  const refData = referralsData?.data || {};
  const leaderboard = leaderboardData?.data || [];
  const referralsList = refData.referrals || [];

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">CATALYST PROGRAM</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Scale the institutional network & earn status</p>
        </div>
        <div className="flex items-center gap-3">
           <div className="text-right hidden sm:block">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Catalyst IQ</p>
              <div className="flex items-center gap-2">
                 <Award size={14} className="text-amber-500" />
                 <p className="text-sm font-[900] text-slate-900 tracking-tighter uppercase">Elite Ambassador</p>
              </div>
           </div>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
        {/* Main Earning Panel */}
        <div className="xl:col-span-2 space-y-8">
           <div className="glass-panel p-10 lg:p-14 bg-slate-900 text-white overflow-hidden relative group flex flex-col md:flex-row gap-10">
              <div className="absolute top-0 right-0 w-[400px] h-[400px] bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
              
              <div className="relative z-10 flex-1">
                 <div className="flex items-start justify-between mb-12">
                    <div className="w-16 h-16 bg-blue-600 rounded-[24px] flex items-center justify-center shadow-xl shadow-blue-500/20">
                       <Zap size={32} className="text-white" />
                    </div>
                    <div className="text-right">
                       <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Available Rewards</p>
                       <h2 className="text-4xl font-[900] tracking-tighter italic">{refData.rewardPoints || 0} PX</h2>
                    </div>
                 </div>

                 <div className="space-y-4 mb-12">
                    <h3 className="text-3xl font-[900] tracking-tighter italic uppercase leading-none">Expand the Network.<br />Earn Institutional Credits.</h3>
                    <p className="text-slate-400 font-bold text-xs uppercase tracking-widest leading-relaxed max-w-sm">
                       Every verified institutional onboarding adds points to your account. Unlock elite tier features faster.
                    </p>
                 </div>

                 <div className="bg-white/5 border border-white/10 rounded-[32px] p-8 backdrop-blur-md">
                    <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-4">Unique Affiliate Alias</p>
                    <div className="flex items-center gap-4">
                       <div className="flex-1 bg-white/5 border border-white/10 px-6 py-4 rounded-2xl text-xs font-black tracking-tight text-blue-400 font-mono italic truncate">
                          {refData.referralLink || 'Loading...'}
                       </div>
                       <button 
                         onClick={() => handleShare('copy')}
                         className="p-4 bg-white rounded-2xl flex items-center justify-center text-slate-900 shadow-xl hover:scale-105 active:scale-95 transition-all"
                         title="Copy Link"
                       >
                          <Copy size={18} />
                       </button>
                    </div>
                 </div>
              </div>

              {/* Direct Invite Form */}
              <div className="relative z-10 md:w-80 bg-white/5 border border-white/10 rounded-[32px] p-8 backdrop-blur-md flex flex-col justify-between">
                <div>
                  <h4 className="text-sm font-[900] uppercase italic mb-6">Quick Invite</h4>
                  <form onSubmit={handleInviteSubmit} className="space-y-4">
                    <input 
                      type="text" required placeholder="Contact Name"
                      value={inviteData.name} onChange={e => setInviteData({...inviteData, name: e.target.value})}
                      className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-xs font-bold text-white placeholder:text-slate-400 outline-none focus:bg-white/20 transition-colors"
                    />
                    <input 
                      type="text" required placeholder="Phone Number"
                      value={inviteData.phone} onChange={e => setInviteData({...inviteData, phone: e.target.value})}
                      className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-xs font-bold text-white placeholder:text-slate-400 outline-none focus:bg-white/20 transition-colors"
                    />
                    <input 
                      type="text" placeholder="Firm Name (Optional)"
                      value={inviteData.firmName} onChange={e => setInviteData({...inviteData, firmName: e.target.value})}
                      className="w-full bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-xs font-bold text-white placeholder:text-slate-400 outline-none focus:bg-white/20 transition-colors"
                    />
                    <Button type="submit" disabled={sendingInvite} className="w-full bg-blue-600 hover:bg-blue-700 mt-2 py-4 shadow-xl">
                      {sendingInvite ? <Loader2 size={16} className="animate-spin" /> : <><Send size={14} className="mr-2" /> Send via WhatsApp</>}
                    </Button>
                  </form>
                </div>
              </div>
           </div>

           <div className="glass-panel p-10">
              <div className="flex items-center justify-between mb-10">
                 <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase">My Referrals</h3>
                 <div className="flex gap-4">
                   <div className="text-right">
                     <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Approved</p>
                     <p className="text-sm font-[900] text-green-500 italic">{refData.approvedReferrals || 0}</p>
                   </div>
                   <div className="text-right">
                     <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Pending</p>
                     <p className="text-sm font-[900] text-amber-500 italic">{refData.pendingReferrals || 0}</p>
                   </div>
                 </div>
              </div>
              <div className="space-y-4">
                 {loadingReferrals ? (
                   <p className="text-xs font-bold text-slate-400 text-center py-10 uppercase tracking-widest">Loading history...</p>
                 ) : referralsList.length === 0 ? (
                   <div className="text-center py-10">
                     <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">You haven't referred anyone yet.</p>
                     <p className="text-[10px] text-slate-400 mt-2">Start growing the community!</p>
                   </div>
                 ) : referralsList.map((ref, i) => (
                   <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }} key={i} className="p-6 bg-slate-50/50 rounded-[28px] border border-slate-100 flex items-center justify-between group hover:bg-white hover:shadow-xl hover:border-transparent transition-all">
                      <div className="flex items-center gap-5">
                         <div className="w-12 h-12 rounded-2xl bg-white border border-slate-100 flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors shadow-sm">
                            <Users size={20} />
                         </div>
                         <div>
                            <p className="text-xs font-black text-slate-900 uppercase tracking-wider mb-1">{ref.refereeName}</p>
                            <p className="text-[10px] font-bold text-slate-500 truncate max-w-[200px]">{ref.refereeFirm}</p>
                         </div>
                      </div>
                      <div className="text-right">
                         <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-2">{ref.appliedAt}</p>
                         <Badge status={ref.status === 'APPROVED' ? 'active' : ref.status === 'PENDING' ? 'warning' : 'error'} label={ref.status} />
                      </div>
                   </motion.div>
                 ))}
              </div>
           </div>
        </div>

        {/* Intelligence Sidebar: Leaderboard & Flow */}
        <div className="xl:col-span-1 space-y-10">
           
           <div className="glass-panel p-10 bg-slate-50/50 border-slate-100">
             <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-8">Leaderboard</h3>
             <div className="space-y-6">
               {leaderboard.map((lb, i) => (
                 <div key={i} className={`flex items-center gap-4 ${lb.memberId === 'MEM-001' ? 'bg-white p-4 rounded-2xl shadow-sm border border-blue-100' : 'px-4'}`}>
                   <div className={`w-8 h-8 rounded-lg flex items-center justify-center font-black text-[10px] ${i === 0 ? 'bg-amber-100 text-amber-600' : i === 1 ? 'bg-slate-200 text-slate-600' : i === 2 ? 'bg-orange-100 text-orange-600' : 'bg-slate-100 text-slate-400'}`}>
                     #{lb.rank}
                   </div>
                   <div className="flex-1">
                     <p className="text-[11px] font-black text-slate-900 uppercase truncate">{lb.name}</p>
                     <p className="text-[9px] text-slate-500 font-bold truncate">{lb.firmName}</p>
                   </div>
                   <div className="text-right">
                     <p className="text-xs font-black text-blue-600 italic">{lb.approvedReferrals}</p>
                   </div>
                 </div>
               ))}
             </div>
           </div>

           <div className="glass-panel p-10">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-8">Share Intelligence</h3>
              <div className="grid grid-cols-2 gap-4">
                 <button onClick={() => handleShare('whatsapp')} className="flex flex-col items-center gap-3 p-6 bg-slate-50/50 rounded-2xl border border-slate-100 hover:bg-white hover:shadow-xl hover:border-transparent transition-all group">
                    <div className="w-10 h-10 rounded-xl bg-green-50 border border-green-100 flex items-center justify-center text-green-500 transition-colors shadow-sm">
                       <Share2 size={18} />
                    </div>
                    <span className="text-[9px] font-black text-slate-400 group-hover:text-slate-900 uppercase tracking-widest transition-colors">WhatsApp</span>
                 </button>
                 <button onClick={() => handleShare('copy')} className="flex flex-col items-center gap-3 p-6 bg-slate-50/50 rounded-2xl border border-slate-100 hover:bg-white hover:shadow-xl hover:border-transparent transition-all group">
                    <div className="w-10 h-10 rounded-xl bg-white border border-slate-100 flex items-center justify-center text-slate-400 group-hover:text-blue-600 transition-colors shadow-sm">
                       <Copy size={18} />
                    </div>
                    <span className="text-[9px] font-black text-slate-400 group-hover:text-slate-900 uppercase tracking-widest transition-colors">Copy Link</span>
                 </button>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
}
