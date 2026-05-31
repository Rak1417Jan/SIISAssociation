import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  Calendar, 
  MapPin, 
  Users, 
  ArrowRight, 
  Zap, 
  Star, 
  Clock, 
  Globe,
  Plus,
  Verified,
  ShieldCheck,
  ChevronRight,
  Video,
  AlertTriangle,
  Edit,
  Trash2,
  XCircle,
  Eye
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getEvents, rsvpEvent, createEvent, updateEvent, deleteEvent, cancelEvent, getEventAttendees } from '../services/engagementService';
import PermissionGate from '../components/PermissionGate';

export default function EventManager() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('UPCOMING');
  const [page, setPage] = useState(1);

  const { execute: fetchEvents, data: eventsData, loading } = useApi(getEvents);
  const { execute: doRsvp } = useApi(rsvpEvent);
  const { execute: doDelete } = useApi(deleteEvent);

  useEffect(() => {
    fetchEvents({ status: activeTab }, page, 10);
  }, [activeTab, page, fetchEvents]);

  const handleRsvp = async (eventId, response) => {
    const tid = toast.loading(response === 'GOING' ? 'Securing seat...' : 'Cancelling RSVP...');
    const res = await doRsvp(eventId, 'CURRENT_USER', response);
    if (!res.error) {
      toast.success(res.data.message, { id: tid });
      fetchEvents({ status: activeTab }, page, 10);
    } else {
      toast.error('RSVP Failed', { id: tid });
    }
  };

  const handleDelete = async (eventId) => {
    if (!window.confirm("Are you sure you want to delete this event?")) return;
    const tid = toast.loading("Deleting event...");
    const res = await doDelete(eventId);
    if (!res.error) {
      toast.success("Event deleted", { id: tid });
      fetchEvents({ status: activeTab }, page, 10);
    } else {
      toast.error("Failed to delete event", { id: tid });
    }
  };

  const handleCreate = () => {
    toast.success('Event creation module initialized (Stub)');
  };

  const handleDossier = (title) => {
    toast(`Fetching event dossier for ${title}`, { icon: '📂' });
  };

  const events = eventsData?.data?.data || [];
  const total = eventsData?.data?.total || 0;
  
  const featuredEvent = events.length > 0 && activeTab === 'UPCOMING' ? events[0] : null;
  const gridEvents = featuredEvent ? events.slice(1) : events;

  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">EVENT RADAR</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional gatherings & strategic forums</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex p-1 bg-slate-50 rounded-xl mr-4">
             <button 
               onClick={() => { setActiveTab('UPCOMING'); setPage(1); }}
               className={`px-6 py-2 rounded-lg text-[9px] font-black uppercase tracking-widest transition-all ${activeTab === 'UPCOMING' ? 'bg-white text-slate-900 shadow-xl' : 'text-slate-400 hover:text-slate-600'}`}>
               Upcoming
             </button>
             <button 
               onClick={() => { setActiveTab('PAST'); setPage(1); }}
               className={`px-6 py-2 rounded-lg text-[9px] font-black uppercase tracking-widest transition-all ${activeTab === 'PAST' ? 'bg-white text-slate-900 shadow-xl' : 'text-slate-400 hover:text-slate-600'}`}>
               Past
             </button>
          </div>
          <button 
            onClick={() => toast('Calendar synchronization complete', { icon: '📅' })}
            className="px-6 py-3 bg-slate-50 border border-slate-100 rounded-xl text-[10px] font-black text-slate-400 hover:text-slate-900 transition-all uppercase tracking-widest italic active:scale-95 hidden sm:block"
          >
            Calendar View
          </button>
          <PermissionGate action="create" resource="events">
            <Button size="sm" onClick={handleCreate} className="gap-2 shadow-xl shadow-blue-500/20 active:scale-95">
              <Plus size={16} />
              Create Event
            </Button>
          </PermissionGate>
        </div>
      </div>

      {loading && <div className="py-20 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">Scanning radar...</div>}

      {!loading && events.length === 0 && (
        <div className="py-20 text-center text-xs font-bold text-slate-400 uppercase tracking-widest">No events found in this sector.</div>
      )}

      {/* Featured Intel Unit */}
      {!loading && featuredEvent && (
        <div className="glass-panel p-10 lg:p-14 bg-slate-900 text-white overflow-hidden relative group">
           <div className="absolute top-[-20%] right-[-10%] w-[500px] h-[500px] bg-blue-600/10 blur-[130px] rounded-full pointer-events-none group-hover:scale-110 transition-transform duration-[2000ms]" />
           
           <div className="relative z-10 flex flex-col lg:flex-row lg:items-center justify-between gap-12">
              <div className="flex-1 space-y-6">
                 <div className="flex items-center gap-4">
                    <span className="px-4 py-1.5 bg-blue-600 rounded-lg text-[9px] font-black uppercase tracking-[0.2em] shadow-lg shadow-blue-600/20 italic">Featured {featuredEvent.type}</span>
                    {featuredEvent.status === 'UPCOMING' && featuredEvent.availableSeats > 0 && (
                      <div className="flex items-center gap-2 text-green-500">
                         <Verified size={14} />
                         <span className="text-[9px] font-black uppercase tracking-widest italic">Booking Open</span>
                      </div>
                    )}
                    {featuredEvent.availableSeats === 0 && (
                      <Badge status="rejected" label="FULL" />
                    )}
                 </div>
                 <h2 className="text-4xl lg:text-5xl font-[900] tracking-tighter italic uppercase leading-none">{featuredEvent.title}</h2>
                 <p className="text-slate-400 font-bold text-xs uppercase tracking-widest leading-relaxed max-w-sm">
                    {featuredEvent.description}
                 </p>
                 <div className="flex items-center gap-8 pt-4">
                    <div className="flex items-center gap-2">
                       <Calendar className="text-blue-500" size={16} />
                       <span className="text-xs font-black uppercase tracking-widest font-mono">{featuredEvent.date}</span>
                    </div>
                    <div className="flex items-center gap-2">
                       <MapPin className="text-blue-500" size={16} />
                       <span className="text-xs font-black uppercase tracking-widest font-mono truncate max-w-[200px]">{featuredEvent.venue}</span>
                    </div>
                 </div>
              </div>
              
              <div className="lg:w-[320px] bg-white/5 border border-white/10 rounded-[40px] p-10 backdrop-blur-md">
                 <div className="flex items-center justify-between mb-8">
                    <p className="text-[10px] font-black uppercase tracking-widest text-slate-500">Booking Status</p>
                    <Zap className={featuredEvent.myRsvp === 'GOING' ? "text-green-400" : "text-amber-400"} size={20} />
                 </div>
                 <div className="space-y-4 mb-10">
                    <div className="flex justify-between items-baseline">
                       <span className="text-xs font-black uppercase tracking-widest text-slate-400 italic">Access</span>
                       <span className="text-2xl font-[900] italic">{featuredEvent.isFree ? 'FREE' : `₹${featuredEvent.ticketPrice}`}</span>
                    </div>
                    
                    <div className="flex justify-between items-center pt-2">
                       <span className="text-[9px] font-bold text-slate-500 uppercase tracking-widest">Seats Left</span>
                       <span className={`text-sm font-black ${featuredEvent.availableSeats < 10 ? 'text-red-400' : 'text-slate-300'}`}>{featuredEvent.availableSeats}</span>
                    </div>
                 </div>

                 {featuredEvent.status === 'UPCOMING' && (
                   featuredEvent.myRsvp === 'GOING' ? (
                     <div className="space-y-3">
                       <Button variant="secondary" className="w-full bg-green-500/20 text-green-400 hover:bg-green-500/30 font-black border-transparent">
                          ✓ Going
                       </Button>
                       <button onClick={() => handleRsvp(featuredEvent.id, 'NOT_GOING')} className="w-full text-center text-[9px] font-black text-slate-500 hover:text-white uppercase tracking-widest transition-colors">
                         Cancel RSVP
                       </button>
                       {featuredEvent.isOnline && featuredEvent.meetLink && (
                         <Button onClick={() => window.open(featuredEvent.meetLink, '_blank')} className="w-full mt-2 bg-blue-600 hover:bg-blue-700">
                           <Video size={14} className="mr-2" /> Join Meeting
                         </Button>
                       )}
                     </div>
                   ) : (
                     <Button 
                        disabled={featuredEvent.availableSeats === 0}
                        onClick={() => handleRsvp(featuredEvent.id, 'GOING')}
                        className="w-full bg-white text-slate-900 hover:bg-slate-100 font-black text-[10px] uppercase tracking-[0.2em] py-5 rounded-2xl shadow-2xl active:scale-95 disabled:opacity-50"
                     >
                       {featuredEvent.availableSeats === 0 ? 'Fully Booked' : 'RSVP Now'} <ArrowRight size={14} className="ml-2" />
                     </Button>
                   )
                 )}
              </div>
           </div>
        </div>
      )}

      {/* Main Events Grid */}
      {!loading && gridEvents.length > 0 && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
           {gridEvents.map((e, i) => (
             <motion.div 
               key={e.id}
               initial={{ opacity: 0, y: 20 }}
               animate={{ opacity: 1, y: 0 }}
               transition={{ delay: i * 0.1 }}
               className="glass-panel p-10 group hover:shadow-[0_40px_80px_-20px_rgba(59,130,246,0.1)] transition-all duration-500 border-slate-100 flex flex-col justify-between relative"
             >
                <div className="space-y-6">
                   <div className="flex items-center justify-between">
                      <Badge status={e.isFree ? 'active' : 'info'} label={e.type} />
                      <div className="text-right">
                         <p className="text-[9px] font-black text-slate-300 uppercase tracking-widest leading-none mb-1">Fee</p>
                         <p className="text-xs font-black text-slate-900 italic uppercase">{e.isFree ? 'FREE' : `₹${e.ticketPrice}`}</p>
                      </div>
                   </div>
                   
                   <div>
                      <h3 className="text-xl font-[900] text-slate-900 tracking-tight uppercase italic mb-3 group-hover:text-blue-600 transition-colors">{e.title}</h3>
                      <p className="text-[11px] font-bold text-slate-400 leading-relaxed uppercase tracking-wider line-clamp-2">{e.description}</p>
                   </div>

                   <div className="space-y-3 pt-6 border-t border-slate-50">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                           <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400 shadow-inner">
                              <Calendar size={14} />
                           </div>
                           <span className="text-[10px] font-black text-slate-900 uppercase tracking-[0.1em]">{e.date} • {e.time}</span>
                        </div>
                        {e.availableSeats > 0 && e.availableSeats < 10 && e.status === 'UPCOMING' && (
                          <span className="text-[9px] font-bold text-red-500 uppercase tracking-widest flex items-center gap-1"><AlertTriangle size={10} /> Filling Fast</span>
                        )}
                      </div>
                      <div className={`p-4 rounded-xl flex items-center gap-3 transition-colors ${e.isOnline ? 'bg-purple-50 text-purple-600' : 'bg-slate-50 text-slate-400'}`}>
                         <MapPin size={14} />
                         <span className="text-[10px] font-black uppercase tracking-widest truncate">{e.venue}</span>
                      </div>
                   </div>
                </div>

                <div className="mt-8 space-y-3">
                   {e.status === 'UPCOMING' && (
                     e.myRsvp === 'GOING' ? (
                        <div className="flex gap-2">
                           <Button variant="secondary" className="flex-1 bg-green-50 text-green-600 border-green-100 hover:bg-green-100">
                             ✓ Going
                           </Button>
                           <Button variant="secondary" onClick={() => handleRsvp(e.id, 'NOT_GOING')} className="px-4">
                             Cancel
                           </Button>
                        </div>
                     ) : (
                        <Button 
                          disabled={e.availableSeats === 0} 
                          onClick={() => handleRsvp(e.id, 'GOING')} 
                          className="w-full"
                        >
                          {e.availableSeats === 0 ? 'FULLY BOOKED' : 'RSVP NOW'}
                        </Button>
                     )
                   )}
                   
                   <PermissionGate action="delete" resource="events">
                     <div className="flex justify-between items-center pt-4 border-t border-slate-50">
                       <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">Admin Controls</span>
                       <div className="flex gap-2">
                         <button onClick={() => toast('Edit module (Stub)')} className="p-2 text-slate-400 hover:text-blue-600"><Edit size={14} /></button>
                         <button onClick={() => toast('View attendees (Stub)')} className="p-2 text-slate-400 hover:text-slate-900"><Eye size={14} /></button>
                         <button onClick={() => handleDelete(e.id)} className="p-2 text-slate-400 hover:text-red-600"><Trash2 size={14} /></button>
                       </div>
                     </div>
                   </PermissionGate>
                </div>
             </motion.div>
           ))}
        </div>
      )}

      <div className="glass-panel p-8 bg-blue-50/50 border-blue-100 flex items-center justify-between">
         <div className="flex items-center gap-4">
            <ShieldCheck size={24} className="text-blue-600" />
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">All events are fully synchronized with our institutional portal</p>
         </div>
      </div>
    </div>
  );
}
