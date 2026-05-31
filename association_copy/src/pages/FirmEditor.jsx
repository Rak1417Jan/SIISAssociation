import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Building2, 
  MapPin, 
  Globe, 
  Users, 
  ShieldCheck, 
  ArrowLeft, 
  Save, 
  Trash2,
  Image as ImageIcon,
  Link as LinkIcon,
  Phone,
  Mail,
  ChevronRight,
  TrendingUp,
  Receipt,
  Plus
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate, useSearchParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getFirmById, updateFirm, deleteFirm, getFirmMembers, addMemberToFirm, removeMemberFromFirm } from '../services/firmService';
import { searchMembers } from '../services/memberService';


export default function FirmEditor() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const id = searchParams.get('id');

  const { execute: fetchFirm, data: firmRes, loading: firmLoading } = useApi(getFirmById);
  const { execute: doUpdate, loading: updateLoading } = useApi(updateFirm);
  const { execute: doDelete } = useApi(deleteFirm);
  const { execute: fetchMembers, data: membersRes } = useApi(getFirmMembers);
  const { execute: doAddMember } = useApi(addMemberToFirm);
  const { execute: doRemoveMember } = useApi(removeMemberFromFirm);
  const { execute: doSearchMembers } = useApi(searchMembers);

  const [formData, setFormData] = useState({
     name: '', registrationNo: '', address: '', website: '', email: '', phone: ''
  });

  const [searchMemberQuery, setSearchMemberQuery] = useState('');
  const [searchResults, setSearchResults] = useState([]);

  useEffect(() => {
     if (id) {
        fetchFirm(id).then(res => {
           if (res.data) {
              setFormData({
                 name: res.data.name || '',
                 registrationNo: res.data.registrationNo || '',
                 address: res.data.address || '',
                 website: res.data.website || '',
                 email: res.data.email || '',
                 phone: res.data.phone || ''
              });
           }
        });
        fetchMembers(id);
     }
     // eslint-disable-next-line
  }, [id]);

  const handleSave = async () => {
    if (!id) return;
    const tid = toast.loading('Synchronizing...');
    const res = await doUpdate(id, formData);
    if (!res.error) toast.success('Institutional record synchronized', {id: tid});
    else toast.error('Failed to update', {id: tid});
  };

  const handleDelete = async () => {
    if (!window.confirm('Delete this firm permanently?')) return;
    const tid = toast.loading('Deleting...');
    const res = await doDelete(id);
    if (!res.error) {
       toast.success('Firm deleted', {id: tid});
       navigate('/firms');
    } else {
       toast.error('Failed to delete', {id: tid});
    }
  };

  const handleMemberSearch = async (val) => {
     setSearchMemberQuery(val);
     if (!val) { setSearchResults([]); return; }
     const res = await doSearchMembers(val);
     if (res.data) setSearchResults(res.data);
  };

  const handleAddMember = async (memberId) => {
     const tid = toast.loading('Linking member...');
     const res = await doAddMember(id, memberId);
     if (!res.error) {
        toast.success('Member linked', {id: tid});
        setSearchMemberQuery('');
        setSearchResults([]);
        fetchMembers(id);
     } else toast.error('Failed to link member', {id: tid});
  };

  const handleRemoveMember = async (memberId) => {
     if (!window.confirm('Remove this member from firm?')) return;
     const tid = toast.loading('Removing member...');
     const res = await doRemoveMember(id, memberId);
     if (!res.error) {
        toast.success('Member removed', {id: tid});
        fetchMembers(id);
     } else toast.error('Failed to remove member', {id: tid});
  };

  const firmData = firmRes?.data;

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-center justify-between pb-6 border-b border-slate-100">
        <button onClick={() => navigate('/firms')} className="flex items-center gap-3 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-[0.2em] transition-colors group">
          <div className="w-10 h-10 bg-slate-50 rounded-xl flex items-center justify-center group-hover:bg-white group-hover:shadow-lg transition-all">
            <ArrowLeft size={16} />
          </div>
          Institutional Index
        </button>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="bg-red-50 text-red-600 border-red-100" onClick={handleDelete}>
             <Trash2 size={14} />
          </Button>
          <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20" onClick={handleSave} disabled={updateLoading}>
            {updateLoading ? <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> : <Save size={14} />}
            Commit Record
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-10">
        {/* Left: Identity Panel */}
        <div className="xl:col-span-1 space-y-8">
           <div className="glass-panel p-10 text-center relative overflow-hidden group">
              <div className="absolute top-0 left-0 w-full h-32 bg-indigo-600" />
              <div className="relative pt-12">
                 <div className="relative inline-block">
                    <div className="w-32 h-32 rounded-[32px] bg-white p-2 shadow-2xl relative z-10 mx-auto overflow-hidden">
                       <div className="w-full h-full rounded-[28px] bg-slate-50 flex items-center justify-center text-slate-300">
                          <Building2 size={48} />
                       </div>
                    </div>
                    <button className="absolute -bottom-2 -right-2 w-10 h-10 bg-white border border-slate-100 rounded-2xl flex items-center justify-center text-indigo-600 shadow-xl z-20 hover:scale-110 transition-transform">
                       <Plus size={18} />
                    </button>
                 </div>
                 <h2 className="text-2xl font-[900] text-slate-900 mt-6 tracking-tight uppercase italic">{firmData?.name || 'Firm Profile'}</h2>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1 italic">{firmData?.id || 'FRM-...'}</p>
                 <div className="flex justify-center mt-6">
                    <Badge status={(firmData?.status || 'active').toLowerCase()} label={firmData?.status || 'Active'} />
                 </div>
              </div>

              <div className="mt-8 pt-8 border-t border-slate-50 grid grid-cols-2 gap-4">
                 <div className="text-center">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Human Capital</p>
                    <p className="text-lg font-[900] text-slate-900 tracking-tighter italic">{firmData?.memberCount || 0} ENTITIES</p>
                 </div>
                 <div className="text-center">
                    <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Market IQ</p>
                    <p className="text-lg font-[900] text-green-500 tracking-tighter italic">84.2%</p>
                 </div>
              </div>
           </div>

           <div className="glass-panel p-8">
              <h3 className="text-sm font-black text-slate-900 uppercase tracking-widest mb-6 italic">Intel Mapping</h3>
              <div className="space-y-4">
                 {[
                   { label: 'Market Share', value: '4.2%', drift: '+0.5%' },
                   { label: 'Network Density', value: 'High', drift: 'Stable' },
                   { label: 'Compliance Index', value: '98/100', drift: 'Legacy' },
                 ].map((stat, i) => (
                    <div key={i} className="p-4 bg-slate-50 rounded-2xl flex justify-between items-center">
                       <div>
                          <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">{stat.label}</p>
                          <p className="text-sm font-[900] text-slate-900 italic tracking-tighter">{stat.value}</p>
                       </div>
                       <span className="text-[9px] font-black text-blue-600 uppercase tracking-widest">{stat.drift}</span>
                    </div>
                 ))}
              </div>
           </div>
        </div>

        {/* Right: Detailed Fields */}
        <div className="xl:col-span-2 space-y-8">
           <div className="glass-panel p-10 lg:p-14">
              <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase mb-10">Institutional Profile Editor</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-8">
                  {[
                   { label: 'Entity Legal Name', placeholder: 'Enter firm name', icon: Building2, key: 'name' },
                   { label: 'GST Identity Number', placeholder: '27AAAAA0000A1Z5', icon: ShieldCheck, key: 'registrationNo' },
                   { label: 'Headquarters Region', placeholder: 'Mumbai, MH', icon: MapPin, key: 'address' },
                   { label: 'Global Website', placeholder: 'www.kumarind.org', icon: Globe, key: 'website' },
                   { label: 'Authorized Email', placeholder: 'info@kumarind.org', icon: Mail, key: 'email' },
                   { label: 'Institutional Liaison', placeholder: '+91 22 8849 0101', icon: Phone, key: 'phone' },
                 ].map((field, i) => (
                    <div key={i} className="space-y-2">
                       <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest ml-1">{field.label}</label>
                       <div className="relative group">
                          <field.icon size={16} className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-300 group-focus-within:text-indigo-600 transition-colors" />
                          <input 
                            type="text" 
                            value={formData[field.key]}
                            onChange={(e) => setFormData({...formData, [field.key]: e.target.value})}
                            placeholder={field.placeholder}
                            className="w-full pl-14 pr-6 py-4 bg-slate-50/50 border border-slate-100 rounded-[20px] text-xs font-black outline-none focus:border-indigo-500 focus:bg-white transition-all transition-duration-300"
                          />
                       </div>
                    </div>
                 ))}
              </div>

              <div className="mt-16 pt-10 border-t border-slate-50">
                 <h4 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-8">Affiliated Members</h4>
                 
                 {/* Search and Add */}
                 <div className="flex gap-4 mb-6 relative">
                    <input 
                       type="text" 
                       value={searchMemberQuery} 
                       onChange={e => handleMemberSearch(e.target.value)} 
                       placeholder="Search member by name to add..." 
                       className="flex-1 px-6 py-4 bg-slate-50 border border-slate-100 rounded-[20px] text-xs font-black outline-none focus:border-indigo-500 transition-all" 
                    />
                    {searchResults.length > 0 && (
                       <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-2xl shadow-xl border border-slate-100 z-50 p-2 max-h-[300px] overflow-y-auto">
                          {searchResults.map(m => (
                            <div key={m.id} className="p-4 flex items-center justify-between hover:bg-slate-50 rounded-xl cursor-pointer" onClick={() => handleAddMember(m.id)}>
                               <div><p className="text-sm font-[900] text-slate-900 uppercase">{m.name}</p><p className="text-[10px] font-black text-slate-400">{m.id}</p></div>
                               <Plus size={16} className="text-indigo-600" />
                            </div>
                          ))}
                       </div>
                    )}
                 </div>
                 
                 {/* Table */}
                 <div className="bg-white border border-slate-100 rounded-[24px] overflow-hidden">
                    <table className="w-full text-left">
                       <thead className="bg-slate-50/50">
                          <tr>
                             <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Member</th>
                             <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Contact</th>
                             <th className="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">Actions</th>
                          </tr>
                       </thead>
                       <tbody className="divide-y divide-slate-50">
                          {membersRes?.data?.data?.length > 0 ? membersRes.data.data.map(m => (
                             <tr key={m.id} className="group hover:bg-slate-50/30">
                                <td className="px-6 py-4">
                                   <p className="text-xs font-[900] text-slate-900 uppercase">{m.name}</p>
                                   <p className="text-[10px] font-black text-slate-400">{m.id}</p>
                                </td>
                                <td className="px-6 py-4">
                                   <p className="text-[10px] font-bold text-slate-600 uppercase">{m.email}</p>
                                   <p className="text-[10px] font-bold text-slate-600">{m.phone}</p>
                                </td>
                                <td className="px-6 py-4 text-right">
                                   <button onClick={() => handleRemoveMember(m.id)} className="text-slate-300 hover:text-red-500 hover:bg-red-50 p-2 rounded-lg transition-colors"><Trash2 size={16} /></button>
                                </td>
                             </tr>
                          )) : (
                             <tr><td colSpan="3" className="p-8 text-center text-[10px] font-bold uppercase tracking-widest text-slate-400">No members affiliated</td></tr>
                          )}
                       </tbody>
                    </table>
                 </div>
              </div>
                 <h4 className="text-sm font-black text-slate-900 uppercase tracking-widest italic mb-8">Business Trajectory</h4>
                 <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    <div className="p-8 bg-slate-900 rounded-[32px] text-white overflow-hidden relative group">
                       <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/20 blur-3xl rounded-full group-hover:scale-125 transition-transform duration-700" />
                       <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1 relative z-10">Annual Revenue Flow</p>
                       <h5 className="text-3xl font-[900] tracking-tighter italic relative z-10">₹4.2M <span className="text-xs font-normal text-green-500">+12%</span></h5>
                       <div className="mt-8 flex items-center justify-between relative z-10">
                          <TrendingUp size={24} className="text-indigo-500" />
                          <button className="text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-white transition-colors">Ledger Context</button>
                       </div>
                    </div>

                    <div className="p-8 bg-indigo-600 rounded-[32px] text-white overflow-hidden relative group">
                        <div className="absolute top-[-20%] left-[-20%] w-32 h-32 bg-white/10 blur-3xl rounded-full" />
                        <p className="text-[10px] font-black text-indigo-200 uppercase tracking-widest mb-1 relative z-10">Personnel Growth</p>
                        <h5 className="text-3xl font-[900] tracking-tighter italic relative z-10">42 <span className="text-xs font-normal text-indigo-300">Staff Assets</span></h5>
                        <div className="mt-8 flex items-center justify-between relative z-10">
                           <Users size={24} className="text-white" />
                           <button className="text-[10px] font-black uppercase tracking-widest text-white/50 hover:text-white transition-colors">Shift Roster</button>
                        </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>
      </div>
  );
}
