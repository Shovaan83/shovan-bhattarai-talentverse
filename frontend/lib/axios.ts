import axios from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5249/api",
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 30000, // 30 seconds timeout for email operations
  withCredentials: true, // CRITICAL: Send cookies with every request
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

// ⭐ Token refresh state management
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

// ⭐ Response interceptor: Auto-refresh on 401 Unauthorized
axiosInstance.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    // Only redirect on 401 for protected routes, not auth pages
    const authPages = ['/login', '/register', '/forgot-password'];
    const isAuthPage = typeof window !== "undefined" && authPages.some(page => window.location.pathname.includes(page));
    
    // If 401 and not already retrying and not on auth page
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthPage) {
      if (isRefreshing) {
        // Another request is already refreshing, queue this one
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return axiosInstance(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // ⭐ Attempt to refresh token
        const response = await axiosInstance.post<{ success: boolean; data: string }>('/account/refresh');

        if (response.data.success) {
          const newToken = response.data.data;
          localStorage.setItem('token', newToken);
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          processQueue(null, newToken);
          return axiosInstance(originalRequest);
        } else {
          // Refresh failed, redirect to login
          processQueue(new Error('Token refresh failed'), null);
          localStorage.removeItem("token");
          localStorage.removeItem("rememberMe");
          localStorage.removeItem("userEmail");
          if (typeof window !== "undefined") {
            window.location.href = "/login";
          }
          return Promise.reject(error);
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        processQueue(refreshError, null);
        localStorage.removeItem("token");
        localStorage.removeItem("rememberMe");
        localStorage.removeItem("userEmail");
        if (typeof window !== "undefined") {
          window.location.href = "/login";
        }
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default axiosInstance;
