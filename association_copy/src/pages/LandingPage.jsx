import React from 'react';
import { motion } from 'framer-motion';
import { 
  ShieldCheck, 
  Globe, 
  Zap, 
  ArrowRight, 
  Users, 
  Building2, 
  ChevronDown,
  Star,
  Award,
  MousePointer2
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';

const features = [
  { 
    title: 'Institutional Network', 
    desc: 'Access a verified network of 4,000+ industry leaders and corporate entities globally.',
    icon: Globe,
    color: 'text-blue-500 bg-blue-50'
  },
  { 
    title: 'Elite Credentials', 
    desc: 'Receive digital and physical institutional credentials tracked via a secure ledger.',
    icon: ShieldCheck,
    color: 'text-green-500 bg-green-50'
  },
  { 
    title: 'Market Intelligence', 
    desc: 'Direct access to business analytics, regional saturation indexing, and growth forecasts.',
    icon: Zap,
    color: 'text-amber-500 bg-amber-50'
  }
];

export default function LandingPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-[#f8fafc] selection:bg-blue-100 selection:text-blue-900 overflow-x-hidden">
      {/* Dynamic Background */}
      <div className="absolute inset-0 z-0 overflow-hidden pointer-events-none">
        <div className="absolute top-[-10%] right-[-5%] w-[40%] h-[40%] bg-blue-500/10 blur-[130px] rounded-full" />
        <div className="absolute bottom-[-5%] left-[-5%] w-[30%] h-[30%] bg-purple-500/10 blur-[130px] rounded-full" />
        <div className="absolute top-[30%] left-[10%] w-[15%] h-[15%] bg-indigo-500/5 blur-[80px] rounded-full" />
        <div className="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-[0.03] contrast-150 brightness-100" />
        <div 
           className="absolute inset-0 opacity-[0.02]" 
           style={{ backgroundImage: `radial-gradient(#1e293b 0.5px, transparent 0.5px)`, backgroundSize: '24px 24px' }} 
        />
      </div>

      {/* Navigation */}
      <nav className="relative z-50 flex items-center justify-between px-10 py-8 max-w-7xl mx-auto">
         <div className="flex items-center gap-3 group cursor-pointer" onClick={() => navigate('/')}>
            <div className="w-10 h-10 bg-slate-900 rounded-xl flex items-center justify-center font-black text-[10px] text-white shadow-xl shadow-slate-900/10 transition-transform group-hover:scale-105">VIA</div>
            <div className="flex flex-col">
               <span className="text-xs font-[900] text-slate-900 tracking-tighter italic">ASSOCIATION</span>
               <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none">Institutional Portal</span>
            </div>
         </div>
         <div className="flex items-center gap-6">
            <button 
              onClick={() => navigate('/login')}
              className="text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-widest transition-colors"
            >
              Sign In
            </button>
            <Button 
               onClick={() => navigate('/register')}
               className="px-8 py-3.5 bg-slate-900 hover:bg-blue-600 text-white rounded-2xl text-[10px] font-black uppercase tracking-[0.2em] shadow-2xl shadow-slate-900/10 transition-all active:scale-95"
            >
               Sign Up
            </Button>
         </div>
      </nav>

      {/* Hero Section */}
      <section className="relative z-10 pt-20 pb-32 px-10 max-w-7xl mx-auto text-center">
         <motion.div
           initial={{ opacity: 0, y: 20 }}
           animate={{ opacity: 1, y: 0 }}
           transition={{ duration: 0.8 }}
         >
            <div className="inline-flex items-center gap-3 px-4 py-2 bg-blue-50 border border-blue-100 rounded-full mb-8">
               <Award size={14} className="text-blue-600" />
               <span className="text-[10px] font-black text-blue-600 uppercase tracking-widest italic">Institutional Trust Standard 2026</span>
            </div>
            <h1 className="text-6xl md:text-8xl font-[900] text-slate-900 tracking-tighter italic uppercase leading-[0.9] mb-10">
               Institutional<br />
               <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 via-purple-600 to-indigo-600">Excellence.</span>
            </h1>
            <p className="text-lg lg:text-xl font-bold text-slate-400 uppercase tracking-widest max-w-2xl mx-auto leading-relaxed mb-16">
               The global operating system for industry leaders, providing verified credentials, networking intelligence, and executive support.
            </p>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-6">
               <Button 
                 onClick={() => navigate('/register')}
                 className="px-12 py-6 rounded-[24px] bg-blue-600 hover:bg-blue-700 text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/20 group h-auto"
               >
                 Start Onboarding <ArrowRight size={18} className="ml-3 group-hover:translate-x-1 transition-transform" />
               </Button>
            </div>
         </motion.div>

         {/* Visual Hero Feature */}
         <motion.div 
           initial={{ opacity: 0, scale: 0.9, y: 40 }}
           animate={{ opacity: 1, scale: 1, y: 0 }}
           transition={{ delay: 0.4, duration: 1 }}
           className="mt-32 relative"
         >
            <div className="glass-panel p-4 bg-white/50 backdrop-blur-2xl border-slate-100/50 shadow-[0_80px_160px_-40px_rgba(0,0,0,0.08)] overflow-hidden rounded-[50px]">
               <div className="bg-slate-900 rounded-[40px] aspect-[16/10] lg:aspect-[16/8] flex items-center justify-center relative overflow-hidden group">
                  <div className="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-[0.05] pointer-events-none" />
                  <div className="absolute top-0 right-0 w-full h-full bg-[radial-gradient(circle_at_80%_20%,rgba(59,130,246,0.15)_0%,transparent_50%)]" />
                  
                  <div className="text-center relative z-10 space-y-8">
                     <div className="w-24 h-24 bg-white/5 rounded-[32px] flex items-center justify-center mx-auto border border-white/10 group-hover:scale-110 transition-transform duration-700">
                        <Users size={40} className="text-blue-500" />
                     </div>
                     <div className="space-y-2">
                        <p className="text-6xl font-[900] text-white tracking-tighter italic">4,280+</p>
                        <p className="text-[10px] font-black text-slate-500 uppercase tracking-[0.4em]">Verified Active Institutional Entities</p>
                     </div>
                  </div>

                  {/* Floating Action Badge */}
                  <div className="absolute bottom-10 right-10 bg-white p-6 rounded-[32px] shadow-2xl flex items-center gap-4 group-hover:scale-110 transition-transform">
                     <div className="w-10 h-10 bg-green-50 rounded-xl flex items-center justify-center text-green-600">
                        <ShieldCheck size={20} />
                     </div>
                     <div className="text-left">
                        <p className="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Security Score</p>
                        <p className="text-sm font-[900] text-slate-900 italic tracking-tighter">PHASE_ALPHA</p>
                     </div>
                  </div>
               </div>
            </div>
         </motion.div>
      </section>

      {/* Features Grid */}
      <section className="py-32 px-10 max-w-7xl mx-auto border-t border-slate-100">
         <div className="text-center mb-24">
            <p className="text-[10px] font-black text-blue-600 uppercase tracking-[0.4em] mb-4 italic">Core Infrastructure</p>
            <h2 className="text-4xl font-[900] text-slate-900 tracking-tighter italic uppercase">Engineered for Scale.</h2>
         </div>
         <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            {features.map((f, i) => (
              <motion.div 
                key={i}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.2 }}
                className="glass-panel p-12 group hover:bg-white transition-all duration-500"
              >
                 <div className={`w-14 h-14 rounded-2xl ${f.color} flex items-center justify-center mb-10 shadow-inner group-hover:scale-110 transition-transform`}>
                    <f.icon size={26} />
                 </div>
                 <h3 className="text-xl font-[900] text-slate-900 tracking-tight italic uppercase mb-4">{f.title}</h3>
                 <p className="text-[11px] font-bold text-slate-400 uppercase tracking-widest leading-relaxed">
                   {f.desc}
                 </p>
              </motion.div>
            ))}
         </div>
      </section>

      {/* CTA Footer */}
      <section className="py-40 px-10 relative overflow-hidden bg-slate-900 text-white">
         <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-blue-600/10 blur-[130px] rounded-full" />
         <div className="max-w-4xl mx-auto text-center relative z-10">
            <h2 className="text-5xl md:text-7xl font-[900] tracking-tighter italic uppercase mb-12">Ready to Synchronize?</h2>
            <p className="text-slate-400 font-bold text-sm uppercase tracking-widest leading-relaxed mb-16 max-w-xl mx-auto">
               Begin your institutional onboarding protocol today and unlock the full potential of global industry networking.
            </p>
            <div className="flex justify-center">
               <Button 
                 onClick={() => navigate('/register')}
                 className="px-16 py-8 rounded-[32px] bg-white text-slate-900 hover:bg-blue-600 hover:text-white font-[900] text-lg uppercase tracking-[0.2em] shadow-2xl transition-all h-auto"
               >
                 Register Repository
               </Button>
            </div>
         </div>
      </section>

      <footer className="py-12 border-t border-slate-100/10 bg-slate-900 text-slate-500 flex flex-col items-center gap-6">
         <div className="flex items-center gap-2">
            <span className="text-[9px] font-[900] text-white italic tracking-tighter">VIA</span>
            <span className="text-[9px] font-black uppercase tracking-widest">ASSOCIATION © 2026</span>
         </div>
         <p className="text-[8px] font-bold uppercase tracking-[0.4em]">Institutional Integrity Synchronized Globally</p>
      </footer>
    </div>
  );
}
