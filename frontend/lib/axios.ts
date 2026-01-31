import axios from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5249/api",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 30000, // 30 seconds timeout for email operations
});

// Add request interceptor to include auth token
axiosInstance.interceptors.request.use(
  (config) => {
    // Don't add auth header for public endpoints
    const publicEndpoints = ['/account/login', '/account/register', '/account/forgot-password', '/account/reset-password', '/account/login-2fa'];
    const isPublicEndpoint = publicEndpoints.some(endpoint => config.url?.includes(endpoint));
    
    if (!isPublicEndpoint) {
      const token = localStorage.getItem("token");
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor for better error handling
axiosInstance.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Only redirect on 401 for protected routes, not auth pages
    const authPages = ['/login', '/register', '/forgot-password'];
    const isAuthPage = typeof window !== "undefined" && authPages.some(page => window.location.pathname.includes(page));
    
    if (error.response?.status === 401 && !isAuthPage) {
      // Token expired or invalid
      localStorage.removeItem("token");
      localStorage.removeItem("rememberMe");
      localStorage.removeItem("userEmail");
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
