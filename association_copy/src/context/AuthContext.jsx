import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { getCurrentUser } from '../services/authService';

const AuthContext = createContext(null);
const USER_STORAGE_KEY = 'amms_user';

function readStoredUser() {
  try {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    if (raw) {
      return JSON.parse(raw);
    }
  } catch {
    /* ignore */
  }
  const role = localStorage.getItem('userRole');
  if (role) {
    return {
      id: '',
      name: '',
      email: '',
      role,
    };
  }
  return null;
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('token') || null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const validateSession = async () => {
      const storedToken = localStorage.getItem('token');

      if (!storedToken) {
        setIsLoading(false);
        return;
      }

      setToken(storedToken);

      const cachedUser = readStoredUser();
      if (cachedUser) {
        setUser(cachedUser);
      }

      try {
        const result = await getCurrentUser();
        if (result.data) {
          setUser(result.data);
          localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(result.data));
          if (result.data.role) {
            localStorage.setItem('userRole', result.data.role);
          }
        } else if (cachedUser) {
          setUser(cachedUser);
        }
      } catch {
        if (cachedUser) {
          setUser(cachedUser);
        }
      } finally {
        setIsLoading(false);
      }
    };

    validateSession();
  }, []);

  const login = useCallback((userData, authToken) => {
    const role = userData.role || 'member';
    localStorage.setItem('token', authToken);
    localStorage.setItem('userRole', role);
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(userData));
    setUser(userData);
    setToken(authToken);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
    setToken(null);
  }, []);

  const value = {
    user,
    token,
    isLoading,
    login,
    logout,
    isAuthenticated: !!token && !!user,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

export default AuthContext;
