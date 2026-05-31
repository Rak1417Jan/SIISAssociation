import React, { useState, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  User, 
  Building2, 
  Mail, 
  Phone, 
  MapPin, 
  Calendar, 
  ShieldCheck, 
  Edit3,
  Camera,
  Verified,
  ChevronRight,
  X
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import toast from 'react-hot-toast';

export default function MyInfo() {
  const [loading, setLoading] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [avatar, setAvatar] = useState(null);
  const fileInputRef = useRef(null);

  const userRole = localStorage.getItem('userRole') || 'member';
  const isAdmin = userRole === 'admin';

  const [securitySettings, setSecuritySettings] = useState([
    { label: 'Cloud Authorization', type: 'Two-Factor', status: 'Enabled', active: true },
    { label: 'Login Alerts', type: 'Session Monitor', status: 'Active', active: true },
  ]);

  const [profileData, setProfileData] = useState(
    isAdmin 
      ? {
          firm: 'Test Industries Association',
          id: 'VIA-BK-0001',
          email: 'ravi@testindustries.org',
          phone: '+91 99999 11111',
          hq: 'Delhi, India',
          regDate: '01 Jan 2020'
        }
      : {
          firm: 'Kumar Industry Logistics',
          id: 'VIA-BK-2023-8849',
          email: 'daksh.sharma@viatech.org',
          phone: '+91 98765 43210',
          hq: 'Mumbai, Maharashtra',
          regDate: '14 Oct 2023'
        }
  );

  const displayName = isAdmin ? 'Ravi' : 'Daksh Sharma';
  const displayRole = isAdmin ? 'System Admin' : 'Premium Member';

  const handleUpdate = () => {
    if (!isEditing) return;
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      setIsEditing(false);
      toast.success('Identity synchronized globally');
    }, 1500);
  };

  const toggleSecurity = (index) => {
    const updated = [...securitySettings];
    updated[index].active = !updated[index].active;
    updated[index].status = updated[index].active ? (index === 0 ? 'Enabled' : 'Active') : 'Disabled';
    setSecuritySettings(updated);
    toast.success(`${updated[index].label} ${updated[index].status.toLowerCase()}`);
  };

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatar(reader.result);
        toast.success('Avatar updated successfully');
      };
      reader.readAsDataURL(file);
    }
  };

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">MY PROFILE</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Manage your institutional identity</p>
        </div>
        <Badge status="active" label={isAdmin ? 'System Admin' : 'Verified Member'} />
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8">
        {/* Left Column: Avatar & Basic Stats */}
        <div className="xl:col-span-1 space-y-8">
          <div className="glass-panel p-10 text-center relative overflow-hidden group">
            <div className={`absolute top-0 left-0 w-full h-32 bg-gradient-to-br ${isAdmin ? 'from-indigo-700 to-purple-800' : 'from-blue-600 to-indigo-700'}`} />
            <div className="relative pt-12 pb-6">
              <div className="relative inline-block">
                <div className="w-32 h-32 rounded-[40px] bg-white p-2 shadow-2xl relative z-10 mx-auto overflow-hidden">
                   <div className="w-full h-full rounded-[32px] bg-slate-100 flex items-center justify-center text-slate-300 overflow-hidden">
                     {avatar ? (
                       <img src={avatar} alt="Profile" className="w-full h-full object-cover" />
                     ) : (
                       <User size={48} />
                     )}
                   </div>
                </div>
                <input 
                  type="file" 
                  ref={fileInputRef} 
                  onChange={handleFileChange} 
                  accept="image/*" 
                  className="hidden" 
                />
                <button 
                  onClick={() => fileInputRef.current.click()}
                  className="absolute -bottom-2 -right-2 w-10 h-10 bg-white border border-slate-100 rounded-2xl flex items-center justify-center text-blue-600 shadow-xl hover:scale-110 active:scale-95 transition-all z-20"
                >
                  <Camera size={18} />
                </button>
              </div>
              <h2 className="text-2xl font-[900] text-slate-900 mt-6 tracking-tight">{displayName}</h2>
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{displayRole}</p>
              
              <div className="flex items-center justify-center gap-2 mt-4 text-green-500 font-black text-[10px] uppercase tracking-widest">
                 <Verified size={14} />
                 Identity Verified
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4 mt-8 pt-8 border-t border-slate-50">
               <div className="text-center">
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Joined</p>
                 <p className="text-sm font-[900] text-slate-800 uppercase tracking-tighter">{isAdmin ? 'JAN 2020' : 'OCT 2023'}</p>
               </div>
               <div className="text-center">
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Points</p>
                 <p className="text-sm font-[900] text-blue-600 uppercase tracking-tighter">{isAdmin ? '∞' : '2,480'}</p>
               </div>
            </div>
          </div>

          <div className="glass-panel p-8">
             <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-6">Security Settings</h3>
             <div className="space-y-4">
                {securitySettings.map((s, i) => (
                  <button 
                    key={i} 
                    onClick={() => toggleSecurity(i)}
                    className="w-full flex items-center justify-between p-4 bg-slate-50/50 hover:bg-white rounded-2xl border border-slate-100 transition-all text-left active:scale-[0.98]"
                  >
                    <div>
                      <p className="text-[10px] font-black text-slate-900 uppercase tracking-widest">{s.label}</p>
                      <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{s.type}</p>
                    </div>
                    <span className={`text-[9px] font-black italic tracking-widest uppercase ${s.active ? 'text-blue-600' : 'text-slate-400'}`}>
                      {s.status}
                    </span>
                  </button>
                ))}
             </div>
          </div>
        </div>

        {/* Right Column: Information Form */}
        <div className="xl:col-span-2 space-y-8">
          <div className="glass-panel p-10 lg:p-14 transition-all duration-500">
            <div className="flex items-center justify-between mb-10">
              <h3 className="text-2xl font-[900] text-slate-900 tracking-tight">Institutional Profile</h3>
              <Button 
                variant={isEditing ? "default" : "secondary"} 
                size="sm" 
                onClick={() => setIsEditing(!isEditing)}
                className={`gap-2 transition-all ${isEditing ? 'bg-red-500 hover:bg-red-600 text-white shadow-red-500/20' : ''}`}
              >
                {isEditing ? (
                  <><X size={14} /> Cancel Edit</>
                ) : (
                  <><Edit3 size={14} /> Edit Mode</>
                )}
              </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-10">
               {[
                 { icon: Building2, label: 'Firm Name', key: 'firm', editable: true },
                 { icon: ShieldCheck, label: 'Membership ID', key: 'id', editable: false },
                 { icon: Mail, label: 'System Email', key: 'email', editable: true },
                 { icon: Phone, label: 'Direct Mobile', key: 'phone', editable: true },
                 { icon: MapPin, label: 'Headquarters', key: 'hq', editable: true },
                 { icon: Calendar, label: 'Registration Date', key: 'regDate', editable: false },
               ].map((field, i) => (
                 <div key={i} className="relative group">
                    <div className={`absolute left-[-1rem] top-0 bottom-0 w-[2px] transition-colors ${isEditing && field.editable ? 'bg-blue-400' : 'bg-slate-100 group-hover:bg-blue-600'}`} />
                    <div className="flex items-center gap-3 mb-2">
                      <field.icon size={16} className={`transition-colors ${isEditing && field.editable ? 'text-blue-500' : 'text-slate-300'}`} />
                      <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">{field.label}</p>
                    </div>
                    {isEditing && field.editable ? (
                      <input 
                        type="text"
                        value={profileData[field.key]}
                        onChange={(e) => setProfileData({...profileData, [field.key]: e.target.value})}
                        className="w-full bg-slate-50 border-2 border-slate-200 rounded-xl px-4 py-3 text-sm font-black text-slate-900 outline-none focus:border-blue-500 focus:bg-white transition-all shadow-inner"
                      />
                    ) : (
                      <p className="text-base font-[900] text-slate-800 tracking-tight h-[44px] flex items-center">{profileData[field.key]}</p>
                    )}
                 </div>
               ))}
            </div>

            <div className="mt-16 pt-10 border-t border-slate-50">
               <h3 className="text-sm font-black text-slate-300 uppercase tracking-widest mb-8">Business Address</h3>
               <div className="p-8 bg-slate-50/50 rounded-[32px] border border-slate-100">
                  <p className="text-sm font-bold text-slate-600 leading-relaxed max-w-lg uppercase tracking-wider">
                     {isAdmin ? (
                       <>Association HQ, Tower A, Connaught Place, <br />New Delhi - 110001, India</>
                     ) : (
                       <>Building 14, Central Logistics Hub, Sector 5, <br />Near Terminal 2, MIDC Industrial Area, <br />Mumbai - 400093, Maharashtra, India</>
                     )}
                  </p>
               </div>
            </div>

            <AnimatePresence mode="wait">
              {isEditing && (
                <motion.div 
                  initial={{ opacity: 0, height: 0, marginTop: 0 }}
                  animate={{ opacity: 1, height: 'auto', marginTop: 48 }}
                  exit={{ opacity: 0, height: 0, marginTop: 0 }}
                  className="flex justify-end overflow-hidden"
                >
                   <Button 
                     onClick={handleUpdate} 
                     disabled={loading}
                     className="px-10 py-5 rounded-[22px] btn-premium text-white font-black text-sm uppercase tracking-widest flex items-center gap-3 shadow-xl"
                   >
                     {loading ? <span className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : 'Synchronize Identity'}
                     <ChevronRight size={18} />
                   </Button>
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </div>
      </div>
    </div>
  );
}
