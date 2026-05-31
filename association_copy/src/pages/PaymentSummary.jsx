import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { CreditCard, Shield, CheckCircle, ArrowRight, Receipt, ChevronRight, Lock, Verified } from 'lucide-react';
import { Button } from '../components/ui/Button';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { createRazorpayOrder, verifyPayment } from '../services/paymentService';
import { openRazorpayModal } from '../services/razorpayService';

export default function PaymentSummary() {
  const [showModal, setShowModal] = useState(false);
  const [processing, setProcessing] = useState(false);
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const { execute: doCreateOrder } = useApi(createRazorpayOrder);
  const { execute: doVerify } = useApi(verifyPayment);

  useEffect(() => {
    const stored = localStorage.getItem('amms_selected_plan');
    if (stored) setPlan(JSON.parse(stored));
  }, []);

  const handleConfirm = async () => {
    setProcessing(true);
    const orderRes = await doCreateOrder(plan?.id, 'CURRENT_USER');
    if (orderRes.error) {
      toast.error('Could not create order. Try again.');
      setProcessing(false);
      return;
    }
    openRazorpayModal(
      orderRes.data,
      async (paymentResponse) => {
        const verifyRes = await doVerify(
          paymentResponse.razorpayOrderId,
          paymentResponse.razorpayPaymentId,
          paymentResponse.razorpaySignature
        );
        setProcessing(false);
        setShowModal(false);
        if (!verifyRes.error) {
          toast.success('Payment successful!');
          navigate('/payment-success', { state: { payment: verifyRes.data } });
        } else {
          toast.error('Verification failed. Contact support.');
        }
      },
      (errMsg) => {
        setProcessing(false);
        setShowModal(false);
        toast.error(errMsg || 'Payment failed', {
          duration: 5000,
          icon: '💳',
        });
      }
    );
  };

  return (
    <div className="page-wrapper pt-12">
      <motion.div 
        initial={{ opacity: 0, y: 20 }} 
        animate={{ opacity: 1, y: 0 }} 
        className="max-w-2xl mx-auto px-6"
      >
        <div className="mb-10">
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight mb-2">Final Summary</h1>
          <p className="text-slate-400 font-bold text-sm uppercase tracking-widest leading-none">Review your billing information</p>
        </div>

        <div className="bg-white rounded-[40px] p-10 lg:p-14 shadow-[0_40px_80px_-20px_rgba(59,130,246,0.1)] border border-slate-50 relative overflow-hidden">
          <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 blur-3xl rounded-full" />
          
          <div className="space-y-8">
            <div className="flex items-center justify-between pb-8 border-b border-slate-100">
               <div>
                 <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Membership Plan</p>
                 <h3 className="text-xl font-[900] text-blue-600">{plan ? `${plan.name} Membership` : 'Loading...'}</h3>
               </div>
               <div className="bg-blue-50 text-blue-600 px-4 py-2 rounded-full font-black text-[10px] uppercase tracking-widest">
                 Selected
               </div>
            </div>

            <div className="space-y-4">
              {[
                { label: 'Base Membership Fee', value: plan ? `₹${plan.price.toLocaleString()}.00` : '—' },
                { label: `GST @ ${plan?.gstPercent || 18}%`, value: plan ? `₹${plan.gstAmount.toLocaleString()}.00` : '—' },
                { label: 'Platform Fee', value: plan ? `₹${plan.platformFee.toLocaleString()}.00` : '—' },
              ].map(row => (
                <div key={row.label} className="flex justify-between items-center">
                  <span className="text-sm font-bold text-slate-400 uppercase tracking-widest">{row.label}</span>
                  <span className="text-sm font-[900] text-slate-700 font-mono tracking-tight">{row.value}</span>
                </div>
              ))}
            </div>

            <div className="pt-8 border-t border-slate-100 flex items-center justify-between">
              <div>
                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Grand Total</p>
                <div className="flex items-baseline gap-2">
                  <span className="text-sm font-black text-slate-900">₹</span>
                  <span className="text-4xl font-[900] text-slate-900 tracking-tighter">{plan ? plan.totalAmount.toLocaleString() : '—'}</span>
                </div>
              </div>
              <div className="w-16 h-16 bg-slate-50 rounded-[20px] flex items-center justify-center text-slate-300">
                <Receipt size={28} />
              </div>
            </div>
          </div>
        </div>

        <div className="mt-8 flex items-start gap-4 p-6 bg-green-50/50 rounded-[32px] border border-green-100/50">
          <Shield className="text-green-500 shrink-0" size={24} />
          <div>
            <p className="text-xs font-black text-green-700 uppercase tracking-widest mb-1">Encrypted Payment Gateway</p>
            <p className="text-[11px] font-bold text-green-600/80 leading-relaxed uppercase tracking-wider">
               You are about to be redirected to our PCI-DSS compliant secure vault for final transaction authorization.
            </p>
          </div>
        </div>

        <div className="mt-10 flex flex-col items-center gap-6">
          <Button 
            className="w-full py-6 rounded-[24px] btn-premium text-white font-[900] text-sm uppercase tracking-[0.2em] shadow-2xl flex items-center justify-center gap-4 group"
            onClick={() => setShowModal(true)}
          >
            Authorize Payment
            <ChevronRight size={20} className="group-hover:translate-x-1 transition-transform" />
          </Button>
          <button className="text-[10px] font-black text-slate-400 hover:text-slate-900 uppercase tracking-[0.3em] transition-colors">
            Cancel and Return
          </button>
        </div>

        {/* Payment Confirmation Modal */}
        <AnimatePresence>
          {showModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center p-6">
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="absolute inset-0 bg-slate-900/60 backdrop-blur-[8px]"
                onClick={() => !processing && setShowModal(false)}
              />
              <motion.div
                initial={{ scale: 0.9, opacity: 0, y: 20 }}
                animate={{ scale: 1, opacity: 1, y: 0 }}
                exit={{ scale: 0.9, opacity: 0, y: 10 }}
                className="relative bg-white rounded-[48px] p-12 max-w-md w-full shadow-[0_50px_100px_-20px_rgba(0,0,0,0.3)] text-center border border-slate-100 overflow-hidden"
              >
                <div className="absolute top-0 left-0 w-full h-[6px] bg-blue-600" />
                <div className="w-20 h-20 bg-blue-50 rounded-[32px] flex items-center justify-center mx-auto mb-8 shadow-inner">
                  <CreditCard size={32} className="text-blue-600" />
                </div>
                <h3 className="text-2xl font-[900] text-slate-900 tracking-tight mb-3 italic">AUTHORIZE</h3>
                <p className="text-slate-400 font-bold text-sm mb-10 leading-relaxed uppercase tracking-wider">
                  System check complete. Confirming <span className="text-blue-600">{plan ? `₹${plan.totalAmount.toLocaleString()}.00` : '...'}</span> for your membership account.
                </p>
                <Button 
                  className="w-full py-5 rounded-[22px] btn-premium text-white font-black text-xs uppercase tracking-widest" 
                  onClick={handleConfirm} 
                  disabled={processing}
                >
                  {processing
                    ? <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin mx-auto" />
                    : 'Confirm Transaction'
                  }
                </Button>
                {!processing && (
                  <button onClick={() => setShowModal(false)} className="mt-8 text-[10px] font-black text-slate-400 hover:text-slate-800 uppercase tracking-widest">Abort Process</button>
                )}
              </motion.div>
            </div>
          )}
        </AnimatePresence>
      </motion.div>
    </div>
  );
}
