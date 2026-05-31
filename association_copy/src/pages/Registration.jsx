import React, { useState, useEffect, useRef } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/ui/Button';
import { 
  Building2, 
  User, 
  FileText, 
  CheckCircle, 
  ChevronRight, 
  ChevronLeft,
  Mail,
  Phone,
  Calendar,
  CloudUpload,
  ArrowRight,
  ShieldCheck,
  MapPin
} from 'lucide-react';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { startRegistration, saveStep, submitRegistration } from '../services/registrationService';
import { uploadDocument, aiVerifyDocument } from '../services/documentService';

const STEPS = [
  { id: 1, title: 'Business', icon: Building2 },
  { id: 2, title: 'Profile', icon: User },
  { id: 3, title: 'Documents', icon: FileText },
  { id: 4, title: 'Review', icon: ShieldCheck },
];

export default function Registration() {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [dir, setDir] = useState(1);
  const [form, setForm] = useState({
    firmName: '', gst: '', address: '', city: '', state: '',
    firstName: '', lastName: '', email: '', phone: '', dob: '',
  });
  const [files, setFiles] = useState({ aadhar: null, photo: null });

  const [appId, setAppId] = useState(null);
  const [loadingStep, setLoadingStep] = useState(false);
  const [aiResults, setAiResults] = useState({ AADHAR: null, PHOTO: null });
  const aadharRef = useRef();
  const photoRef = useRef();

  useEffect(() => {
    startRegistration(form).then(res => {
      if (res.data) setAppId(res.data.applicationId);
    });
    // eslint-disable-next-line
  }, []);

  const go = async (next) => {
    if (next > step) {
      setLoadingStep(true);
      const res = await saveStep(step, appId, form);
      setLoadingStep(false);
      if (res.error) {
        toast.error('Validation failed. Please check your inputs.');
        return;
      }
    }
    setDir(next > step ? 1 : -1);
    setStep(next);
  };

  const update = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const handleSubmit = async () => {
    setLoadingStep(true);
    const res = await submitRegistration(appId);
    setLoadingStep(false);
    if (res.error) {
      toast.error('Submission failed');
    } else {
      toast.success('Registration data saved!');
      navigate('/status', { state: { applicationId: appId } });
    }
  };

  const handleUpload = async (e, type) => {
    const file = e.target.files[0];
    if (!file || !appId) return;
    toast.loading(`Uploading ${type}...`, { id: 'upload' });
    const uploadRes = await uploadDocument(file, type, appId);
    if (uploadRes.data?.documentId) {
       toast.loading(`Verifying ${type} with AI...`, { id: 'upload' });
       const aiRes = await aiVerifyDocument(uploadRes.data.documentId);
       toast.dismiss('upload');
       if (aiRes.data) {
         setAiResults(prev => ({ ...prev, [type]: aiRes.data }));
       }
    } else {
       toast.dismiss('upload');
       toast.error(`Failed to upload ${type}`);
    }
  };

  return (
    <div className="min-h-screen w-full bg-[#f8faff] overflow-hidden relative flex items-center justify-center font-['Plus_Jakarta_Sans',sans-serif] py-20">
      {/* ── Background Elements ────────────────────────── */}
      <div className="absolute inset-0 bg-[linear-gradient(to_right,#e5e7eb_1px,transparent_1px),linear-gradient(to_bottom,#e5e7eb_1px,transparent_1px)] bg-[size:4rem_4rem] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_50%,#000_70%,transparent_100%)] opacity-30" />
      
      {/* Blurry Orbs */}
      <div className="absolute top-[5%] right-[5%] w-96 h-96 bg-blue-400/10 rounded-full blur-[100px] animate-pulse" />
      <div className="absolute bottom-[5%] left-[5%] w-72 h-72 bg-purple-400/10 rounded-full blur-[100px] animate-pulse" />

      <motion.div 
        initial={{ opacity: 0, y: 20 }} 
        animate={{ opacity: 1, y: 0 }} 
        className="w-full max-w-4xl px-6 relative z-10"
      >
        {/* Progress Header */}
        <div className="flex flex-col items-center mb-14 text-center">
          <div className="w-16 h-16 bg-[#3b82f6] rounded-[20px] flex items-center justify-center mb-6 shadow-xl shadow-blue-500/20">
            <span className="text-white font-[900] text-xl tracking-tighter">VIA</span>
          </div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight mb-3">Become a Member</h1>
          <p className="text-slate-400 font-bold text-sm max-w-sm">
            Join the Industry's most elite association in 4 simple steps
          </p>
        </div>

        {/* Custom Stepper */}
        <div className="flex items-center justify-between mb-12 px-8 relative">
          <div className="absolute h-[2px] bg-slate-100 top-5 left-12 right-12 z-0">
             <motion.div 
               className="h-full bg-blue-600"
               animate={{ width: `${((step - 1) / (STEPS.length - 1)) * 100}%` }}
               transition={{ duration: 0.5 }}
             />
          </div>
          {STEPS.map((s) => (
            <div key={s.id} className="relative z-10 flex flex-col items-center gap-3">
              <motion.div
                animate={{
                  backgroundColor: s.id <= step ? '#2563eb' : '#fff',
                  color: s.id <= step ? '#fff' : '#94a3b8',
                  borderColor: s.id <= step ? '#2563eb' : '#f1f5f9',
                  scale: s.id === step ? 1.2 : 1,
                }}
                className="w-10 h-10 rounded-full flex items-center justify-center border-2 transition-all duration-300 shadow-xl shadow-white"
              >
                {s.id < step ? <CheckCircle size={20} /> : <s.icon size={18} />}
              </motion.div>
              <span className={`text-[10px] font-black uppercase tracking-widest ${s.id <= step ? 'text-blue-600' : 'text-slate-400'}`}>
                {s.title}
              </span>
            </div>
          ))}
        </div>

        {/* Form Card */}
        <div className="bg-white rounded-[48px] p-10 lg:p-14 shadow-[0_40px_80px_-20px_rgba(59,130,246,0.1)] border border-slate-50 min-h-[500px] flex flex-col justify-between">
          <AnimatePresence mode="wait">
            <motion.div
              key={step}
              initial={{ opacity: 0, x: dir * 30 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -dir * 30 }}
              transition={{ duration: 0.4 }}
              className="flex-1"
            >
              {step === 1 && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div className="col-span-2 mb-4">
                    <h2 className="text-2xl font-[900] text-slate-900 tracking-tight">Firm Details</h2>
                    <p className="text-slate-400 font-bold text-sm">Tell us about your organization</p>
                  </div>
                  <div className="col-span-2 relative group">
                    <input type="text" value={form.firmName} onChange={e=>update('firmName', e.target.value)} placeholder="Firm / Agency Name" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none transition-all focus:border-blue-500/50" />
                    <Building2 className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                  <div className="col-span-2 md:col-span-1 relative">
                    <input type="text" value={form.gst} onChange={e=>update('gst', e.target.value)} placeholder="GST Number" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none focus:border-blue-500/50" />
                    <ShieldCheck className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                  <div className="col-span-2 md:col-span-1 relative">
                    <input type="text" value={form.city} onChange={e=>update('city', e.target.value)} placeholder="City" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none focus:border-blue-500/50" />
                    <MapPin className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                </div>
              )}

              {step === 2 && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                  <div className="col-span-2 mb-4">
                    <h2 className="text-2xl font-[900] text-slate-900 tracking-tight">Personal Profile</h2>
                    <p className="text-slate-400 font-bold text-sm">Owner or representative information</p>
                  </div>
                  <div className="relative">
                    <input type="text" value={form.firstName} onChange={e=>update('firstName', e.target.value)} placeholder="First Name" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none focus:border-blue-500/50" />
                    <User className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                  <div className="relative">
                    <input type="text" value={form.lastName} onChange={e=>update('lastName', e.target.value)} placeholder="Last Name" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none focus:border-blue-500/50" />
                    <User className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                  <div className="col-span-2 relative">
                    <input type="email" value={form.email} onChange={e=>update('email', e.target.value)} placeholder="Email Address" className="w-full pl-14 pr-6 py-5 bg-[#f8fafc] border border-slate-200 rounded-[24px] text-slate-800 font-bold text-sm outline-none focus:border-blue-500/50" />
                    <Mail className="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" size={20} />
                  </div>
                </div>
              )}

              {step === 3 && (
                <div className="space-y-8">
                  <div className="mb-4">
                    <h2 className="text-2xl font-[900] text-slate-900 tracking-tight">Document Verification</h2>
                    <p className="text-slate-400 font-bold text-sm">Upload clear copies of the following documents</p>
                  </div>
                  <div onClick={() => aadharRef.current?.click()} className="border-[3px] border-dashed border-slate-100 rounded-[32px] p-10 text-center hover:border-blue-500/50 transition-all cursor-pointer group relative">
                    <input type="file" className="hidden" ref={aadharRef} onChange={(e) => handleUpload(e, 'AADHAR')} />
                    <div className="w-16 h-16 bg-slate-50 rounded-[20px] flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform">
                      <CloudUpload size={28} className="text-slate-300 group-hover:text-blue-600" />
                    </div>
                    <p className="text-slate-800 font-[900] text-lg uppercase tracking-tight">Aadhar Card Copy</p>
                    <p className="text-slate-400 font-bold text-xs mt-1 uppercase tracking-widest">Front & Back (Combined PDF preferred)</p>
                    {aiResults.AADHAR && (
                       <div className="mt-4 flex justify-center items-center gap-2">
                         {aiResults.AADHAR.isValid ? <CheckCircle className="text-green-500" size={16}/> : <span className="text-red-500 font-bold">X</span>}
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-500">AI Confidence: {(aiResults.AADHAR.confidence * 100).toFixed(0)}%</span>
                       </div>
                    )}
                  </div>
                  <div onClick={() => photoRef.current?.click()} className="border-[3px] border-dashed border-slate-100 rounded-[32px] p-10 text-center hover:border-blue-500/50 transition-all cursor-pointer group relative">
                    <input type="file" className="hidden" ref={photoRef} onChange={(e) => handleUpload(e, 'PHOTO')} />
                    <div className="w-16 h-16 bg-slate-50 rounded-[20px] flex items-center justify-center mx-auto mb-4 group-hover:scale-110 transition-transform">
                      <User size={28} className="text-slate-300 group-hover:text-blue-600" />
                    </div>
                    <p className="text-slate-800 font-[900] text-lg uppercase tracking-tight">Passport Photo</p>
                    <p className="text-slate-400 font-bold text-xs mt-1 uppercase tracking-widest">High resolution, clear background</p>
                    {aiResults.PHOTO && (
                       <div className="mt-4 flex justify-center items-center gap-2">
                         {aiResults.PHOTO.isValid ? <CheckCircle className="text-green-500" size={16}/> : <span className="text-red-500 font-bold">X</span>}
                         <span className="text-[10px] font-black uppercase tracking-widest text-slate-500">AI Confidence: {(aiResults.PHOTO.confidence * 100).toFixed(0)}%</span>
                       </div>
                    )}
                  </div>
                </div>
              )}

              {step === 4 && (
                <div className="space-y-8">
                   <div className="mb-4 text-center">
                    <div className="w-20 h-20 bg-green-50 rounded-full flex items-center justify-center mx-auto mb-6">
                      <ShieldCheck size={40} className="text-green-500" />
                    </div>
                    <h2 className="text-2xl font-[900] text-slate-900 tracking-tight">Almost there!</h2>
                    <p className="text-slate-400 font-bold text-sm max-w-sm mx-auto">Please review your information carefully before finalizing your application.</p>
                  </div>
                  <div className="bg-slate-50/50 rounded-[32px] p-8 border border-slate-100">
                    <div className="space-y-4">
                      <div className="flex justify-between items-center pb-4 border-b border-slate-100">
                        <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Application Type</span>
                        <span className="text-xs font-[900] text-slate-800 uppercase tracking-widest">New Industry Member</span>
                      </div>
                      <div className="flex justify-between items-center pb-4 border-b border-slate-100">
                        <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Verification Status</span>
                        <span className="text-xs font-[900] text-amber-500 uppercase tracking-widest">Pending Submission</span>
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </motion.div>
          </AnimatePresence>

          {/* Navigation Buttons */}
          <div className="mt-16 flex items-center justify-between">
            {step > 1 ? (
              <button 
                onClick={() => go(step - 1)}
                className="flex items-center gap-2 text-[10px] font-black text-slate-400 hover:text-slate-900 transition-colors uppercase tracking-[0.2em]"
              >
                <ChevronLeft size={16} />
                Previous Step
              </button>
            ) : (
              <div />
            )}

            <Button
              className="px-10 py-5 rounded-[22px] bg-blue-600 hover:bg-blue-700 text-white font-black text-xs uppercase tracking-widest flex items-center justify-center gap-4 shadow-xl shadow-blue-500/20 min-w-[200px]"
              onClick={step < 4 ? () => go(step + 1) : handleSubmit}
              disabled={loadingStep}
            >
              {loadingStep ? (
                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <>
                  {step < 4 ? 'Continue' : 'Submit Application'}
                  <ChevronRight size={18} />
                </>
              )}
            </Button>
          </div>
        </div>

        <div className="mt-12 text-center text-slate-400 font-bold text-[10px] uppercase tracking-[0.3em]">
          Official Membership Registration Portal
        </div>
      </motion.div>
    </div>
  );
}
