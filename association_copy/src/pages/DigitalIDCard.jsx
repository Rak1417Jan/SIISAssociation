import React from 'react';
import { motion } from 'framer-motion';
import { 
  Download, 
  Share2, 
  ShieldCheck, 
  User, 
  Building2, 
  MapPin, 
  QrCode,
  Globe,
  Zap,
  MoreHorizontal,
  Verified
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getDigitalId, generateDigitalId, downloadDigitalId, verifyDigitalId, shareDigitalId } from '../services/digitalIdService';

export default function DigitalIDCard() {
  const navigate = useNavigate();
  const userRole = localStorage.getItem('userRole') || 'member';
  const isAdmin = userRole === 'admin';
  const memberId = 'MEM-001';

  const { execute: fetchId, data: idData, loading: idLoading } = useApi(getDigitalId);
  const { execute: doGenerate } = useApi(generateDigitalId);
  const { execute: doDownload } = useApi(downloadDigitalId);
  const { execute: doShare } = useApi(shareDigitalId);
  const { execute: doVerify } = useApi(verifyDigitalId);

  const [polling, setPolling] = React.useState(false);

  React.useEffect(() => {
    let interval;
    const checkId = async () => {
      const res = await fetchId(memberId);
      if (res.data?.isGenerated === false && !polling) {
         setPolling(true);
         await doGenerate(memberId);
      } else if (res.data?.isGenerated === true) {
         setPolling(false);
         if (interval) clearInterval(interval);
      }
    };
    checkId();
    if (polling) {
       interval = setInterval(checkId, 5000);
    }
    return () => { if (interval) clearInterval(interval); };
  }, [fetchId, doGenerate, polling]);

  const handleDownload = async () => {
    const tId = toast.loading('Generating Encrypted PDF...');
    const res = await doDownload(memberId, 'pdf');
    if (!res.error) toast.success('Digital ID Card downloaded', {id: tId});
    else toast.error('Download failed', {id: tId});
  };

  const handleShare = async () => {
    const tId = toast.loading('Sending Secure Link...');
    const res = await doShare(memberId, 'whatsapp');
    if (!res.error) toast.success('Copied & Sent via WhatsApp', {id: tId});
    else toast.error('Failed to share', {id: tId});
  };

  const handleVerify = async () => {
    const res = await doVerify(cardData.membershipId);
    if (!res.error && res.data?.isValid) toast.success(`ID Valid for ${res.data.member.name}`);
    else toast.error('ID Verification Failed');
  };

  const cardData = idData?.data || { name: '...', title: '...', id: '...', company: '...', validTill: '...', tier: '...' };

  if (idLoading && !idData) {
     return <div className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">Loading Secure Credentials...</div>;
  }

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">DIGITAL IDENTITY</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Authenticated {isAdmin ? 'admin' : 'member'} credential</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2 active:scale-95" onClick={handleShare}>
            <Share2 size={14} />
            Share ID
          </Button>
          <Button size="sm" className="gap-2 active:scale-95" onClick={handleDownload}>
            <Download size={14} />
            Download PDF
          </Button>
        </div>
      </div>

      <div className="flex flex-col xl:flex-row items-start gap-12">
        {/* The Card Visualization */}
        <div className="w-full max-w-[440px] perspective-1000">
          <motion.div 
            initial={{ rotateY: -15, opacity: 0, scale: 0.9 }}
            animate={{ rotateY: 0, opacity: 1, scale: 1 }}
            whileHover={{ y: -10, rotateY: 5 }}
            transition={{ duration: 0.8, ease: "easeOut" }}
            className={`w-full aspect-[1/1.58] rounded-[40px] shadow-[0_50px_100px_-20px_rgba(0,0,0,0.4)] relative overflow-hidden p-10 text-white border border-white/5 ${
              isAdmin 
                ? 'bg-gradient-to-br from-indigo-900 via-purple-900 to-slate-900' 
                : 'bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900'
            }`}
          >
            {(!cardData.isGenerated || polling) ? (
               <div className="w-full h-full flex flex-col items-center justify-center gap-6 relative z-10 text-center">
                  <div className="w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full animate-spin shadow-xl shadow-blue-500/20" />
                  <p className="text-[10px] font-black uppercase tracking-widest text-slate-300">Generating Secure Credentials...</p>
               </div>
            ) : (
               <>
            {/* Holographic Overlays */}
            <div className="absolute top-0 right-0 w-full h-full bg-[radial-gradient(circle_at_80%_20%,rgba(59,130,246,0.15)_0%,transparent_50%)]" />
            <div className="absolute bottom-0 left-0 w-full h-full bg-[radial-gradient(circle_at_20%_80%,rgba(168,85,247,0.1)_0%,transparent_50%)]" />
            
            {/* Header / Logo */}
            <div className="flex items-center justify-between mb-12 relative z-10">
              <div className="flex items-center gap-3">
                <div className={`w-10 h-10 rounded-xl flex items-center justify-center font-black text-xs tracking-tighter shadow-lg overflow-hidden bg-white`}>
                  {cardData.associationLogo ? <img src={cardData.associationLogo} alt="Logo" className="w-full h-full object-cover" /> : <span className="text-slate-800">VIA</span>}
                </div>
                <div className="flex flex-col">
                  <span className="text-[10px] font-[900] tracking-tighter italic">{cardData.associationName?.toUpperCase() || 'ASSOCIATION'}</span>
                  <span className="text-[8px] font-black text-slate-500 uppercase tracking-widest leading-none">{isAdmin ? 'Administrator' : 'Global Member'}</span>
                </div>
              </div>
              <ShieldCheck className={isAdmin ? 'text-purple-400' : 'text-blue-500'} size={24} />
            </div>

            {/* Profile Photo Area */}
            <div className="flex flex-col items-center mb-10 relative z-10">
              <div className="w-32 h-32 rounded-[32px] bg-white/5 border border-white/10 p-1 mb-6 relative">
                 <div className="w-full h-full rounded-[28px] bg-slate-800 flex items-center justify-center text-slate-700 overflow-hidden">
                    {cardData.photo ? <img src={cardData.photo} alt="Photo" className="w-full h-full object-cover" /> : <User size={48} />}
                 </div>
                 <div className={`absolute -bottom-2 -left-2 w-8 h-8 rounded-full border-4 border-slate-900 ${cardData.status === 'ACTIVE' ? 'bg-green-500' : 'bg-red-500'}`} />
              </div>
              <h2 className="text-2xl font-[900] tracking-tight uppercase italic text-center leading-tight">{cardData.memberName || cardData.name}</h2>
              <p className="text-[9px] font-black text-slate-500 uppercase tracking-[0.2em] mt-1">{cardData.designation || cardData.title}</p>
            </div>

            {/* Credential Data */}
            <div className="grid grid-cols-2 gap-y-6 mb-12 relative z-10">
               <div>
                 <p className="text-[8px] font-black text-slate-500 uppercase tracking-widest mb-1">Membership ID</p>
                 <p className="text-xs font-black tracking-tight">{cardData.membershipId || cardData.id}</p>
               </div>
               <div>
                 <p className="text-[8px] font-black text-slate-500 uppercase tracking-widest mb-1">Company</p>
                 <p className="text-xs font-black tracking-tight">{cardData.firmName || cardData.company}</p>
               </div>
               <div>
                 <p className="text-[8px] font-black text-slate-500 uppercase tracking-widest mb-1">Valid Till</p>
                 <p className="text-xs font-black tracking-tight">{cardData.validUntil || cardData.validTill}</p>
               </div>
               <div>
                 <p className="text-[8px] font-black text-slate-500 uppercase tracking-widest mb-1">Tier Status</p>
                 <p className={`text-xs font-black tracking-tight uppercase ${isAdmin ? 'text-purple-400' : 'text-blue-400'}`}>{cardData.planType || cardData.tier}</p>
               </div>
            </div>

            {/* Bottom Footer */}
            <div className="pt-8 border-t border-white/5 flex items-center justify-between relative z-10">
               <div className="p-3 bg-white rounded-xl shadow-xl cursor-pointer hover:scale-105 transition-transform" onClick={handleVerify}>
                 {cardData.qrCode ? <img src={cardData.qrCode} alt="QR" className="w-10 h-10" /> : <QrCode size={40} className="text-slate-900" />}
               </div>
               <div className="text-right">
                 <p className={`text-[8px] font-black uppercase tracking-widest mb-1 ${cardData.status === 'ACTIVE' ? (isAdmin ? 'text-purple-400' : 'text-blue-500') : 'text-red-500'}`}>System {cardData.status === 'ACTIVE' ? 'Verified' : 'Expired'}</p>
                 <p className="text-[8px] font-bold text-slate-600 uppercase tracking-widest">Global Accreditation Panel</p>
               </div>
            </div>
            </>
            )}
          </motion.div>
        </div>

        {/* Info & Features */}
        <div className="flex-1 space-y-8">
           <div className="glass-panel p-10">
              <h3 className="text-xl font-[900] text-slate-900 tracking-tight mb-6">Digital Features</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                 {[
                   { icon: Globe, label: 'Global Acceptance', text: 'Valid across all international industry hubs.' },
                   { icon: Zap, label: 'Instant Verify', text: 'Real-time credential check via NFC or QR code.' },
                   { icon: ShieldCheck, label: 'Encrypted Identity', text: 'Secured via 256-bit institutional encryption.' },
                   { icon: Verified, label: isAdmin ? 'Admin Badge' : 'Elite Badge', text: isAdmin ? 'Full administrative privileges across all modules.' : 'Exclusive access to VIP industry events.' },
                 ].map((f, i) => (
                   <div key={i} className="flex gap-4">
                      <div className="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-blue-600 shrink-0">
                         <f.icon size={20} />
                      </div>
                      <div>
                        <p className="text-[11px] font-black text-slate-900 uppercase tracking-widest mb-1">{f.label}</p>
                        <p className="text-[11px] font-bold text-slate-400 leading-relaxed uppercase tracking-wider">{f.text}</p>
                      </div>
                   </div>
                 ))}
              </div>
           </div>

           <div className="glass-panel p-8 bg-blue-50/30 border-blue-100 flex items-center justify-between">
              <div className="flex items-center gap-4">
                 <div className="w-12 h-12 rounded-2xl bg-white border border-blue-100 flex items-center justify-center text-blue-600 shadow-sm">
                   <ShieldCheck size={24} />
                 </div>
                 <div>
                   <p className="text-xs font-black text-slate-900 uppercase tracking-widest">NFC Smart-Card</p>
                   <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Request physical version with chip</p>
                 </div>
              </div>
              <Button 
                size="sm" 
                variant="secondary" 
                onClick={() => navigate('/renewal')}
                className="px-6 rounded-xl font-black text-[10px] uppercase tracking-widest"
              >
                Order Delivery
              </Button>
           </div>
        </div>
      </div>
    </div>
  );
}
