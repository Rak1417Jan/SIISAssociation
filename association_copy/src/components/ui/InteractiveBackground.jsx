import React, { useState, useEffect } from 'react';
import { motion, useSpring, useMotionValue } from 'framer-motion';

const CheckboxIcon = ({ checked }) => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" className="w-full h-full opacity-40">
    {checked && <motion.polyline points="20 6 9 17 4 12" initial={{ pathLength: 0 }} animate={{ pathLength: 1 }} transition={{ duration: 1 }} />}
  </svg>
);

const FloatingCheckbox = ({ x, y, size, checked, delay, intensity }) => {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.5 }}
      animate={{ 
        opacity: [0.1, 0.2, 0.1],
        scale: 1,
        y: [0, -15, 0],
        x: [0, 10, 0]
      }}
      transition={{
        y: { duration: 6 + delay, repeat: Infinity, ease: "easeInOut" },
        x: { duration: 8 + delay, repeat: Infinity, ease: "easeInOut" },
        opacity: { duration: 4, repeat: Infinity, ease: "easeInOut" },
        scale: { duration: 1 }
      }}
      style={{
        position: 'absolute',
        left: `${x}%`,
        top: `${y}%`,
        width: size,
        height: size,
        background: 'rgba(255, 255, 255, 0.4)',
        border: '1px solid rgba(255, 255, 255, 0.8)',
        borderRadius: size * 0.25,
        backdropFilter: 'blur(4px)',
        zIndex: 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: size * 0.2,
        filter: size < 20 ? 'blur(1px)' : 'none'
      }}
    >
      <CheckboxIcon checked={checked} />
    </motion.div>
  );
};

export default function InteractiveBackground() {
  const mouseX = useMotionValue(0);
  const mouseY = useMotionValue(0);

  // Smooth springs for parallax
  const springConfig = { damping: 50, stiffness: 300 };
  const smoothX = useSpring(mouseX, springConfig);
  const smoothY = useSpring(mouseY, springConfig);

  // Particles & Checkboxes configuration
  const [elements, setElements] = useState([]);

  useEffect(() => {
    const newElements = Array.from({ length: 15 }).map((_, i) => ({
      id: i,
      x: Math.random() * 100,
      y: Math.random() * 100,
      size: 12 + Math.random() * 24,
      checked: Math.random() > 0.5,
      delay: Math.random() * 5,
      intensity: 0.05 + Math.random() * 0.1
    }));
    setElements(newElements);

    const handleMouseMove = (e) => {
      const { clientX, clientY } = e;
      const x = (clientX / window.innerWidth - 0.5) * 40;
      const y = (clientY / window.innerHeight - 0.5) * 40;
      mouseX.set(x);
      mouseY.set(y);
    };

    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, []);

  return (
    <div className="fixed inset-0 z-0 overflow-hidden bg-[#f8fafc] pointer-events-none">
      {/* LAYER 1: Static Grid Background */}
      <div 
        className="absolute inset-0 opacity-[0.4]" 
        style={{ 
          backgroundImage: `radial-gradient(#e2e8f0 1.5px, transparent 1.5px)`, 
          backgroundSize: '32px 32px' 
        }} 
      />

      {/* LAYER 2: Moving Gradient Layer (Parallax) */}
      <motion.div 
        style={{ 
          x: smoothX,
          y: smoothY,
          scale: 1.2
        }}
        className="absolute inset-[-10%] z-0"
      >
        <div className="absolute top-[20%] left-[20%] w-[60%] h-[60%] bg-blue-500/5 blur-[140px] rounded-full" />
        <div className="absolute bottom-[20%] right-[10%] w-[40%] h-[40%] bg-purple-500/5 blur-[120px] rounded-full" />
      </motion.div>

      {/* Darker Light Source following cursor */}
      <motion.div
        style={{
          x: smoothX,
          y: smoothY,
          translateX: '-50%',
          translateY: '-50%',
          left: '50%',
          top: '50%',
        }}
        className="absolute w-full h-full pointer-events-none opacity-40 z-0"
      >
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,rgba(37,99,235,0.08),transparent_70%)]" />
      </motion.div>

      {/* LAYER 3: Floating Checkboxes + Particles */}
      <motion.div 
        style={{ x: smoothX, y: smoothY }}
        className="absolute inset-0 z-10"
      >
        {elements.map((el) => (
          <FloatingCheckbox 
            key={el.id} 
            {...el} 
          />
        ))}

        {/* Small drifting particles */}
        {Array.from({ length: 40 }).map((_, i) => (
          <motion.div
            key={`p-${i}`}
            className="absolute rounded-full bg-blue-400"
            animate={{
              y: [0, -40, 0],
              x: [0, 20, 0],
              opacity: [0.1, 0.4, 0.1]
            }}
            transition={{
              duration: 10 + Math.random() * 10,
              repeat: Infinity,
              ease: "linear"
            }}
            style={{
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              width: 2 + Math.random() * 4,
              height: 2 + Math.random() * 4,
              opacity: 0.2,
              filter: 'blur(1px)'
            }}
          />
        ))}
      </motion.div>

      {/* Grain Overlay for Premium Feel */}
      <div className="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-[0.03] contrast-150 brightness-100 pointer-events-none scale-150" />
    </div>
  );
}
