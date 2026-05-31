import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  ShieldCheck, Lock, Eye, Edit3, Trash2, Zap, Info, ChevronRight, ShieldAlert, Verified, Check
} from 'lucide-react';
import { Button } from '../components/ui/Button';
import toast from 'react-hot-toast';
import { useApi } from '../hooks/useApi';
import { getRoles, getPermissionMatrix, updateRolePermissions } from '../services/rolesService';

export default function RoleMatrix() {
  const { execute: fetchRoles, data: rolesData } = useApi(getRoles);
  const { execute: fetchMatrix, data: matrixData } = useApi(getPermissionMatrix);
  const { execute: doUpdatePerms } = useApi(updateRolePermissions);

  const [localMatrix, setLocalMatrix] = useState(null);
  const [savingCells, setSavingCells] = useState({});

  useEffect(() => {
    fetchRoles();
    fetchMatrix();
  }, [fetchRoles, fetchMatrix]);

  useEffect(() => {
    if (matrixData?.data) {
      setLocalMatrix(JSON.parse(JSON.stringify(matrixData.data)));
    }
  }, [matrixData]);

  const roles = rolesData?.data || [];
  const matrix = localMatrix || {};

  // Extract all unique resources and their actions from the matrix
  const resourceMap = {};
  Object.values(matrix).forEach(rolePerms => {
    Object.entries(rolePerms).forEach(([resource, actions]) => {
      if (!resourceMap[resource]) resourceMap[resource] = new Set();
      Object.keys(actions).forEach(a => resourceMap[resource].add(a));
    });
  });
  
  const resources = Object.keys(resourceMap).map(res => ({
    name: res,
    actions: Array.from(resourceMap[res])
  }));

  const handleToggle = async (roleName, resource, action, currentValue) => {
    const cellKey = `${roleName}-${resource}-${action}`;
    setSavingCells(prev => ({ ...prev, [cellKey]: true }));
    
    const updatedPerms = JSON.parse(JSON.stringify(matrix[roleName]));
    if (!updatedPerms[resource]) updatedPerms[resource] = {};
    updatedPerms[resource][action] = !currentValue;
    
    // Optimistic UI update
    setLocalMatrix(prev => {
       const next = { ...prev };
       next[roleName] = updatedPerms;
       return next;
    });

    const roleObj = roles.find(r => r.name === roleName);
    const roleId = roleObj ? roleObj.id : roleName;

    const res = await doUpdatePerms(roleId, updatedPerms);
    setSavingCells(prev => ({ ...prev, [cellKey]: false }));
    
    if (!res.error) {
       toast.success('Saved');
    } else {
       toast.error('Failed to save');
       // Revert UI on failure
       setLocalMatrix(prev => {
         const next = { ...prev };
         next[roleName][resource][action] = currentValue;
         return next;
       });
    }
  };
  return (
    <div className="space-y-10">
      {/* Header Context */}
      <div className="flex items-end justify-between pb-6 border-b border-slate-100">
        <div>
          <h1 className="text-4xl font-[900] text-slate-900 tracking-tight italic">ACCESS GOVERNANCE</h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] mt-2">Institutional role matrix & permissioning</p>
        </div>
        <Button size="sm" className="gap-2 shadow-xl shadow-blue-500/20">
          <ShieldAlert size={16} />
          Create Security Role
        </Button>
      </div>

      <div className="glass-panel overflow-hidden border-slate-100">
         <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
               <thead>
                  <tr className="bg-slate-50/50 border-b border-slate-100">
                     <th className="px-8 py-6 text-xs font-black text-slate-900 uppercase tracking-widest sticky left-0 bg-slate-50/95 backdrop-blur-sm z-10 w-64 border-r border-slate-100 shadow-[10px_0_15px_-10px_rgba(0,0,0,0.05)]">
                        Resource / Action
                     </th>
                     {roles.map(role => (
                        <th key={role.id} className="px-8 py-6 text-center border-r border-slate-50 min-w-[140px]">
                           <span className="text-xs font-[900] text-blue-600 tracking-tight italic uppercase block mb-1">{role.displayName}</span>
                           <span className="text-[9px] font-black text-slate-400 uppercase tracking-widest">{role.permissionCount} Perms</span>
                        </th>
                     ))}
                  </tr>
               </thead>
               <tbody className="divide-y divide-slate-50">
                  {resources.map((res, i) => (
                     <React.Fragment key={res.name}>
                        <tr className="bg-slate-50/20">
                           <td colSpan={roles.length + 1} className="px-8 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">
                              {res.name} MODULE
                           </td>
                        </tr>
                        {res.actions.map(action => (
                           <tr key={`${res.name}-${action}`} className="hover:bg-white transition-colors">
                              <td className="px-8 py-5 text-[10px] font-bold text-slate-700 uppercase tracking-widest sticky left-0 bg-white/95 backdrop-blur-sm z-10 border-r border-slate-100 shadow-[10px_0_15px_-10px_rgba(0,0,0,0.05)] pl-12">
                                 {action}
                              </td>
                              {roles.map(role => {
                                 const val = matrix[role.name]?.[res.name]?.[action];
                                 const isSaving = savingCells[`${role.name}-${res.name}-${action}`];
                                 return (
                                    <td key={role.id} className="px-8 py-5 text-center border-r border-slate-50">
                                       {val !== undefined ? (
                                          <button 
                                             disabled={isSaving}
                                             onClick={() => handleToggle(role.name, res.name, action, val)}
                                             className={`w-10 h-6 rounded-full relative transition-colors mx-auto flex items-center ${val ? 'bg-blue-600' : 'bg-slate-200'} ${isSaving ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer hover:opacity-80'}`}
                                          >
                                             <div className={`w-4 h-4 rounded-full bg-white absolute top-1 transition-all flex items-center justify-center ${val ? 'left-5' : 'left-1'}`}>
                                                {isSaving ? (
                                                   <span className="w-2 h-2 border-[1.5px] border-blue-600 border-t-transparent rounded-full animate-spin" />
                                                ) : val && (
                                                   <Check size={10} className="text-blue-600" />
                                                )}
                                             </div>
                                          </button>
                                       ) : (
                                          <span className="text-[10px] text-slate-300 font-bold">—</span>
                                       )}
                                    </td>
                                 );
                              })}
                           </tr>
                        ))}
                     </React.Fragment>
                  ))}
               </tbody>
            </table>
         </div>
      </div>

      {/* Intelligence Sidebar: Governance Status */}
      <div className="glass-panel p-10 bg-slate-900 text-white overflow-hidden relative">
         <div className="absolute top-0 right-10 w-96 h-96 bg-blue-600/10 blur-[120px] rounded-full pointer-events-none" />
         <div className="flex items-center justify-between relative z-10">
            <div className="flex items-center gap-4">
               <div className="w-14 h-14 rounded-2xl bg-white/5 flex items-center justify-center text-blue-500 border border-white/10">
                  <ShieldCheck size={28} />
               </div>
               <div>
                  <h3 className="text-2xl font-[900] tracking-tighter italic uppercase">Security Baseline: ALPHA</h3>
                  <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest mt-1 italic">All institutional permissions are synchronized across 12 zones</p>
               </div>
            </div>
            <Button className="bg-white text-slate-900 hover:bg-slate-100 font-black text-[10px] uppercase tracking-widest border-none shadow-xl">
               Audit Full Matrix
            </Button>
         </div>
      </div>
    </div>
  );
}
