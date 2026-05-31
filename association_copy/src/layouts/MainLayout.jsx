import React from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from '../components/Sidebar';
import Navbar from '../components/Navbar';
import { useSidebar } from '../hooks/useSidebar';
import PageTransition from '../components/PageTransition';

export default function MainLayout() {
  const { collapsed } = useSidebar();
  const location = useLocation();

  return (
    <div className="min-h-screen bg-[#f8fafc]">
      <Sidebar />
      <Navbar />
      
      <main className={`transition-all duration-500 ease-in-out min-h-screen pt-24 px-6 pb-12 ${collapsed ? 'pl-26' : 'pl-78'}`}>
        <div className="max-w-[1600px] mx-auto">
          <PageTransition key={location.pathname}>
            <Outlet />
          </PageTransition>
        </div>
      </main>

      {/* Global Background Decors */}
      <div className="fixed top-[-10%] right-[-5%] w-[40%] h-[40%] bg-blue-100/30 blur-[120px] rounded-full pointer-events-none -z-10" />
      <div className="fixed bottom-[-10%] left-[-5%] w-[30%] h-[30%] bg-indigo-100/20 blur-[120px] rounded-full pointer-events-none -z-10" />
    </div>
  );
}
