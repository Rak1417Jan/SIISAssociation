import React, { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { CheckCircle, ArrowRight, Download, Receipt, Home, ShieldCheck } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate, useLocation } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getPaymentById, generateReceipt } from '../services/paymentService';

export default function PaymentSuccess() {
  const navigate = useNavigate();
  const location = useLocation();
  const passedPayment = location.state?.payment;

  const { execute: fetchPayment, data: paymentRes } = useApi(getPaymentById);
  const { execute: doGenerateReceipt } = useApi(generateReceipt);

  useEffect(() => {
    const id = passedPayment?.paymentId || 'PAY-001';
    fetchPayment(id);
    // eslint-disable-next-line
  }, []);

  const payment = paymentRes?.data || passedPayment;

  const handleDownloadReceipt = async () => {
    const id = payment?.id || payment?.paymentId || 'PAY-001';
    const tid = toast.loading('Generating receipt...');
    const res = await doGenerateReceipt(id);
    if (!res.error) {
      toast.success(`Receipt ${res.data.receiptNo} ready`, {id: tid});
      window.open(res.data.receiptUrl, '_blank');
    } else {
      toast.error('Failed to generate receipt', {id: tid});
    }
  };

  return (
    <div className="page-wrapper flex items-center justify-center py-20 px-6">
      <motion.div 
        initial={{ opacity: 0, scale: 0.9 }} 
        animate={{ opacity: 1, scale: 1 }} 
        className="max-w-xl w-full"
      >
        <div className="bg-white rounded-[48px] p-12 lg:p-16 shadow-[0_50px_100px_-20px_rgba(34,197,94,0.12)] border border-slate-50 text-center relative overflow-hidden">
          {/* Confetti-like Orbs */}
          <div className="absolute top-0 left-0 w-32 h-32 bg-green-500/5 blur-3xl rounded-full" />
          <div className="absolute bottom-0 right-0 w-32 h-32 bg-blue-500/5 blur-3xl rounded-full" />
          
          <motion.div 
            initial={{ scale: 0, rotate: -45 }}
            animate={{ scale: 1, rotate: 0 }}
            transition={{ type: "spring", stiffness: 200, delay: 0.2 }}
            className="w-24 h-24 bg-green-50 rounded-[32px] flex items-center justify-center mx-auto mb-10 shadow-inner"
          >
            <CheckCircle size={48} className="text-green-500" />
          </motion.div>

          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight mb-4">Payment Confirmed</h1>
          <p className="text-slate-400 font-bold text-sm mb-12 uppercase tracking-widest leading-relaxed">
            Welcome to the association. Your membership account is now fully active.
          </p>

          <div className="bg-slate-50/50 rounded-[32px] p-8 border border-slate-100 mb-12 space-y-4">
             <div className="flex justify-between items-center text-[10px] font-black uppercase tracking-widest">
               <span className="text-slate-400">Transaction ID</span>
               <span className="text-slate-800">{payment?.id || payment?.paymentId || '#TXN-...'}</span>
             </div>
             <div className="flex justify-between items-center text-[10px] font-black uppercase tracking-widest">
               <span className="text-slate-400">Receipt No</span>
               <span className="text-slate-800">{payment?.receiptNo || passedPayment?.receiptNo || 'RCP-...'}</span>
             </div>
             <div className="flex justify-between items-center text-[10px] font-black uppercase tracking-widest">
               <span className="text-slate-400">Plan</span>
               <span className="text-slate-800">{payment?.planName || '—'}</span>
             </div>
             <div className="pt-4 border-t border-slate-200/50 flex justify-between items-center">
               <span className="text-sm font-black text-slate-900 uppercase tracking-widest">Amount Paid</span>
               <span className="text-xl font-[900] text-green-600 font-mono tracking-tighter">₹{payment?.totalAmount?.toLocaleString() || '—'}</span>
             </div>
          </div>

          <div className="flex flex-col gap-4">
            <Button 
                onClick={() => navigate('/dashboard')}
                className="w-full py-6 rounded-[24px] btn-premium text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-xl flex items-center justify-center gap-4"
            >
              Go to Dashboard
              <ArrowRight size={20} />
            </Button>
            
            <div className="grid grid-cols-2 gap-4">
                <button onClick={handleDownloadReceipt} className="flex items-center justify-center gap-2 py-4 rounded-2xl bg-white border border-slate-100 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-widest transition-all">
                 <Receipt size={14} />
                 Download Receipt
               </button>
               <button className="flex items-center justify-center gap-2 py-4 rounded-2xl bg-white border border-slate-100 text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-widest transition-all">
                 <ShieldCheck size={14} />
                 ID Card Status
               </button>
            </div>
          </div>
        </div>

        <div className="mt-12 text-center flex flex-col items-center gap-2">
          <div className="flex items-center gap-3">
             <div className="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse" />
             <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em]">Institutional Verification Complete</p>
          </div>
        </div>
      </motion.div>
    </div>
  );
}
