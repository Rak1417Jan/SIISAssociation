// wwwroot/js/api.js

const API_BASE_URL = 'https://localhost:7166/api/v1'; // Update to match your API port if different

// Configure Axios
const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

// Interceptor for attaching auth token
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Interceptor for handling global errors (e.g., 401 Unauthorized)
apiClient.interceptors.response.use(
    (response) => response.data,
    (error) => {
        if (error.response && error.response.status === 401) {
            console.error('Session expired or unauthorized. Redirecting to login...');
            localStorage.removeItem('jwtToken');
            // uncomment the line below to enable automatic redirect on 401
            // window.location.href = '/Login'; 
        }
        return Promise.reject(error);
    }
);

// --- Auth APIs ---
const AuthAPI = {
    login: (data) => apiClient.post('/auth/admin/login', data),
    sendOtp: (data) => apiClient.post('/otp/send', data),
    verifyOtp: (data) => apiClient.post('/otp/verify', data),
    logout: () => apiClient.post('/auth/logout'),
    getSession: () => apiClient.get('/auth/session')
};

// --- Assembly APIs ---
const AssemblyAPI = {
    getList: (activeOnly = true) => apiClient.get(`/assembly/list?activeOnly=${activeOnly}`),
    getById: (id) => apiClient.get(`/assembly/${id}`)
};

// Expose globally so other scripts can access them
window.apiClient = apiClient;
window.AuthAPI = AuthAPI;
window.AssemblyAPI = AssemblyAPI;
