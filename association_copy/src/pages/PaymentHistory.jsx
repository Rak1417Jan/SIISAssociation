import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Download, 
  Search, 
  Filter, 
  ArrowUpRight, 
  FileText, 
  Receipt,
  MoreHorizontal
} from 'lucide-react';
import { Badge } from '../components/ui/Badge';
import { Button } from '../components/ui/Button';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getPaymentHistory, generateReceipt } from '../services/paymentService';

export default function PaymentHistory() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [txnList, setTxnList] = useState([]);
  const [total, setTotal] = useState(0);

  const { execute: fetchHistory, loading } = useApi(getPaymentHistory);
  const { execute: doReceipt } = useApi(generateReceipt);

  useEffect(() => {
    const timer = setTimeout(async () => {
      const res = await fetchHistory('CURRENT_USER', {}, page, 10);
      if (res.data) { setTxnList(res.data.data); setTotal(res.data.total); }
    }, 300);
    return () => clearTimeout(timer);
  }, [page]);

  const totalPages = Math.ceil(total / 10) || 1;

  const handleReceipt = async (id) => {
    const tid = toast.loading('Generating receipt...');
    const res = await doReceipt(id);
    if (!res.error) { toast.success(`Receipt ${res.data.receiptNo} ready`, {id: tid}); window.open(res.data.receiptUrl, '_blank'); }
    else toast.error('Failed', {id: tid});
  };
  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">BILLING LEDGER</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Manage your financial records</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="secondary" size="sm" className="gap-2">
            <Download size={14} />
            Export CSV
          </Button>
          <Button size="sm" className="gap-2">
            <Download size={14} />
            Annual Summary
          </Button>
        </div>
      </div>

      {/* Transaction Table */}
      <div className="glass-panel overflow-hidden border-slate-100">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
           <div className="relative max-w-sm w-full">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
              <input 
                type="text" 
                placeholder="Search transaction ID or type..." 
                className="w-full pl-12 pr-6 py-3 bg-white border border-slate-100 rounded-xl text-xs font-bold outline-none focus:border-blue-500 transition-all shadow-sm"
              />
           </div>
           <div className="flex items-center gap-3">
              <button className="p-3 bg-white border border-slate-100 rounded-xl text-slate-400 hover:text-slate-900 transition-all shadow-sm">
                 <Filter size={18} />
              </button>
              <button className="px-6 py-3 bg-white border border-slate-100 rounded-xl text-[10px] font-black text-slate-400 uppercase tracking-widest hover:text-slate-900 transition-all shadow-sm">
                 Last 12 Months
              </button>
           </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="bg-slate-50/30">
                <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">ID / Date</th>
                <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Transaction Type</th>
                <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-right">Amount</th>
                <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Status</th>
                <th className="px-8 py-5 text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap text-center">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {loading ? (
                <tr><td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">Loading...</td></tr>
              ) : txnList.length === 0 ? (
                <tr><td colSpan="5" className="p-10 text-center text-slate-400 font-bold uppercase tracking-widest text-xs">No transactions found</td></tr>
              ) : txnList.map((txn, i) => (
                <motion.tr 
                  key={txn.id}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.05 }}
                  className="group hover:bg-slate-50/50 transition-colors cursor-pointer"
                >
                  <td className="px-8 py-6">
                    <div className="flex flex-col">
                      <span className="text-xs font-[900] text-slate-900 truncate">#{txn.id}</span>
                      <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{txn.createdAt}</span>
                    </div>
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-white border border-slate-100 rounded-xl flex items-center justify-center text-slate-300 group-hover:text-blue-600 transition-colors shadow-sm">
                        <Receipt size={18} />
                      </div>
                      <div className="flex flex-col">
                        <span className="text-xs font-black text-slate-800 tracking-tight">{txn.planName}</span>
                        <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{txn.receiptNo}</span>
                      </div>
                    </div>
                  </td>
                  <td className="px-8 py-6 text-right">
                    <div className="flex flex-col items-end">
                      <span className="text-sm font-[900] text-slate-900 font-mono italic tracking-tighter">₹{txn.totalAmount?.toLocaleString()}</span>
                      <span className="text-[9px] font-black text-blue-500 uppercase tracking-widest italic pt-1">Authorized</span>
                    </div>
                  </td>
                  <td className="px-8 py-6 text-center">
                    <Badge status={txn.status === 'SUCCESS' ? 'completed' : txn.status.toLowerCase()} label={txn.status} />
                  </td>
                  <td className="px-8 py-6">
                    <div className="flex items-center justify-center gap-2">
                       <button onClick={() => handleReceipt(txn.id)} className="p-2.5 bg-white border border-slate-100 rounded-lg text-slate-300 hover:text-blue-600 transition-all shadow-sm">
                         <Download size={14} />
                       </button>
                    </div>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>
        
        <div className="p-8 bg-slate-50/30 border-t border-slate-100 flex items-center justify-between">
           <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Showing page {page} of {totalPages} • {total} Records</p>
           <div className="flex items-center gap-2">
              <button onClick={() => setPage(p => Math.max(1, p-1))} className="px-4 py-2 border border-slate-200 rounded-lg text-[10px] font-black text-slate-400 uppercase tracking-widest bg-white hover:bg-slate-50 transition-all">Prev</button>
              <button onClick={() => setPage(p => Math.min(totalPages, p+1))} className="px-4 py-2 border border-slate-200 rounded-lg text-[10px] font-black text-slate-900 uppercase tracking-widest bg-white hover:bg-slate-50 transition-all">Next</button>
           </div>
        </div>
      </div>
    </div>
  );
}
